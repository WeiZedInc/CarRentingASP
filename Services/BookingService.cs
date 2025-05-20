using CarRentalSystem.DTOs;
using CarRentalSystem.Models;
using CarRentalSystem.Repositories;

namespace CarRentalSystem.Services
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(int userId, BookingCreateDto bookingDto);
        Task<List<BookingResponseDto>> GetUserBookingsAsync(int userId);
        Task<BookingResponseDto> GetBookingByIdAsync(int bookingId);
        Task<BookingResponseDto> UpdateBookingStatusAsync(int bookingId, string status);
        Task<BookingResponseDto> ExtendBookingAsync(int bookingId, DateTime newReturnDate);
        Task<List<BookingResponseDto>> GetPendingBookingsAsync();
        Task<BookingResponseDto> CancelBookingAsync(int bookingId);
        Task<PaginatedResponse<BookingResponseDto>> GetPaginatedBookingsAsync(int pageNumber, int pageSize);
    }

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILoyaltyProgramRepository _loyaltyRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IEmailService _emailService;

        public BookingService(
            IBookingRepository bookingRepository,
            IVehicleRepository vehicleRepository,
            IUserRepository userRepository,
            ILoyaltyProgramRepository loyaltyRepository,
            IDocumentRepository documentRepository,
            IEmailService emailService)
        {
            _bookingRepository = bookingRepository;
            _vehicleRepository = vehicleRepository;
            _userRepository = userRepository;
            _loyaltyRepository = loyaltyRepository;
            _documentRepository = documentRepository;
            _emailService = emailService;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(int userId, BookingCreateDto bookingDto)
        {
            if (bookingDto.PickupDate >= bookingDto.ReturnDate)
                throw new ArgumentException("Return date must be after pickup date");

            if (bookingDto.PickupDate < DateTime.UtcNow.Date)
                throw new ArgumentException("Pickup date cannot be in the past");

            var vehicle = await _vehicleRepository.GetByIdAsync(bookingDto.VehicleId);
            if (vehicle == null)
                throw new ArgumentException("Vehicle not found");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException("User not found");

            bool isAvailable = await _bookingRepository.IsVehicleAvailableForDatesAsync(
                bookingDto.VehicleId,
                bookingDto.PickupDate,
                bookingDto.ReturnDate
            );

            if (!isAvailable)
                throw new ArgumentException("Vehicle is not available for the selected dates");

            // First check if the user has required documents if this is their first booking
            var hasDocuments = await _documentRepository.UserHasRequiredDocumentsAsync(userId);

            // Calculate total price
            var days = (int)(bookingDto.ReturnDate - bookingDto.PickupDate).TotalDays;
            if (days < 1) days = 1;

            decimal totalPrice = vehicle.DailyRate * days;

            // Apply discount for 3+ days
            if (days >= 3)
            {
                totalPrice = totalPrice * 0.9m; // 10% discount
            }

            // Apply loyalty discount if applicable
            var loyaltyProgram = await _loyaltyRepository.GetUserLoyaltyProgramAsync(userId);
            if (loyaltyProgram != null)
            {
                if (loyaltyProgram.Tier == "Silver")
                    totalPrice = totalPrice * 0.95m; // 5% discount
                else if (loyaltyProgram.Tier == "Gold")
                    totalPrice = totalPrice * 0.9m; // 10% discount
                else if (loyaltyProgram.Tier == "Platinum")
                    totalPrice = totalPrice * 0.85m; // 15% discount
            }

            if (!Enum.TryParse<PaymentMethod>(bookingDto.PaymentMethod, out var paymentMethod))
                throw new ArgumentException("Invalid payment method");

            // Create the booking
            var booking = new Booking
            {
                UserId = userId,
                VehicleId = bookingDto.VehicleId,
                PickupDate = bookingDto.PickupDate,
                ReturnDate = bookingDto.ReturnDate,
                TotalPrice = totalPrice,
                Status = BookingStatus.Requested,
                PickupLocation = bookingDto.PickupLocation,
                ReturnLocation = bookingDto.ReturnLocation,
                PaymentMethod = paymentMethod,
                PaymentStatus = PaymentStatus.Pending,
                BookingDate = DateTime.UtcNow
            };

            var createdBooking = await _bookingRepository.AddAsync(booking);

            // Set vehicle status to Reserved
            vehicle.Status = VehicleStatus.Reserved;
            await _vehicleRepository.UpdateAsync(vehicle);

            // Add loyalty points (10 points per day)
            if (loyaltyProgram != null)
            {
                await _loyaltyRepository.AddLoyaltyPointsAsync(
                    userId,
                    days * 10,
                    $"Booking #{createdBooking.Id} - {days} days rental"
                );
            }

            // Send booking confirmation email
            try
            {
                var user2 = await _userRepository.GetByIdAsync(userId);
                await _emailService.SendBookingConfirmationAsync(createdBooking, user2, vehicle);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send booking confirmation email: {ex.Message}");
            }

            return await MapBookingToDto(createdBooking);
        }

        public async Task<List<BookingResponseDto>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _bookingRepository.GetUserBookingsAsync(userId);
            var bookingDtos = new List<BookingResponseDto>();

            foreach (var booking in bookings)
            {
                bookingDtos.Add(await MapBookingToDto(booking));
            }

            return bookingDtos;
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
            if (booking == null)
                return null;

            return await MapBookingToDto(booking);
        }

        public async Task<BookingResponseDto> UpdateBookingStatusAsync(int bookingId, string status)
        {
            var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
            if (booking == null)
                return null;

            if (!Enum.TryParse<BookingStatus>(status, out var bookingStatus))
                throw new ArgumentException("Invalid booking status");

            booking.Status = bookingStatus;

            // Update vehicle status based on booking status
            var vehicle = await _vehicleRepository.GetByIdAsync(booking.VehicleId);

            if (bookingStatus == BookingStatus.Approved)
            {
                vehicle.Status = VehicleStatus.Reserved;

                // If payment method is "Pay Now", update payment status
                if (booking.PaymentMethod == PaymentMethod.PayNow)
                {
                    booking.PaymentStatus = PaymentStatus.Completed;
                }

                // Send booking approval email
                try
                {
                    var user = await _userRepository.GetByIdAsync(booking.UserId);
                    await _emailService.SendBookingApprovalAsync(booking, user, vehicle);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send booking approval email: {ex.Message}");
                }
            }
            else if (bookingStatus == BookingStatus.Cancelled)
            {
                vehicle.Status = VehicleStatus.Available;

                // If payment was completed, set to refunded
                if (booking.PaymentStatus == PaymentStatus.Completed)
                {
                    booking.PaymentStatus = PaymentStatus.Refunded;
                }

                // Send cancellation email
                try
                {
                    var user = await _userRepository.GetByIdAsync(booking.UserId);
                    await _emailService.SendBookingCancellationAsync(booking, user, vehicle);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send booking cancellation email: {ex.Message}");
                }
            }
            else if (bookingStatus == BookingStatus.Completed)
            {
                vehicle.Status = VehicleStatus.Available;

                // Ensure payment is marked as completed
                booking.PaymentStatus = PaymentStatus.Completed;
            }

            await _vehicleRepository.UpdateAsync(vehicle);
            await _bookingRepository.UpdateAsync(booking);

            return await MapBookingToDto(booking);
        }

        public async Task<BookingResponseDto> ExtendBookingAsync(int bookingId, DateTime newReturnDate)
        {
            var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
            if (booking == null)
                return null;

            if (newReturnDate <= booking.ReturnDate)
                throw new ArgumentException("New return date must be after current return date");

            bool isAvailable = await _bookingRepository.IsVehicleAvailableForDatesAsync(
                booking.VehicleId,
                booking.PickupDate,
                newReturnDate,
                bookingId
            );

            if (!isAvailable)
                throw new ArgumentException("Vehicle is not available for the extended dates");

            // Calculate additional days
            var originalDays = (int)(booking.ReturnDate - booking.PickupDate).TotalDays;
            var newTotalDays = (int)(newReturnDate - booking.PickupDate).TotalDays;
            var additionalDays = newTotalDays - originalDays;

            // Calculate additional price
            var vehicle = await _vehicleRepository.GetByIdAsync(booking.VehicleId);
            decimal additionalPrice = vehicle.DailyRate * additionalDays;

            // Apply discount for 3+ days if applicable
            if (newTotalDays >= 3 && originalDays < 3)
            {
                // Recalculate total price with discount
                decimal newTotalPrice = vehicle.DailyRate * newTotalDays * 0.9m;
                additionalPrice = newTotalPrice - booking.TotalPrice;
            }
            else if (newTotalDays >= 3)
            {
                // Apply discount to additional days
                additionalPrice = additionalPrice * 0.9m;
            }

            // Apply loyalty discount if applicable
            var loyaltyProgram = await _loyaltyRepository.GetUserLoyaltyProgramAsync(booking.UserId);
            if (loyaltyProgram != null)
            {
                if (loyaltyProgram.Tier == "Silver")
                    additionalPrice = additionalPrice * 0.95m;
                else if (loyaltyProgram.Tier == "Gold")
                    additionalPrice = additionalPrice * 0.9m;
                else if (loyaltyProgram.Tier == "Platinum")
                    additionalPrice = additionalPrice * 0.85m;
            }

            // Update booking
            booking.ReturnDate = newReturnDate;
            booking.TotalPrice += additionalPrice;

            await _bookingRepository.UpdateAsync(booking);

            // Add additional loyalty points
            if (loyaltyProgram != null)
            {
                await _loyaltyRepository.AddLoyaltyPointsAsync(
                    booking.UserId,
                    additionalDays * 10,
                    $"Booking #{booking.Id} extension - {additionalDays} additional days"
                );
            }

            return await MapBookingToDto(booking);
        }

        public async Task<List<BookingResponseDto>> GetPendingBookingsAsync()
        {
            var bookings = await _bookingRepository.GetPendingBookingsAsync();
            var bookingDtos = new List<BookingResponseDto>();

            foreach (var booking in bookings)
            {
                bookingDtos.Add(await MapBookingToDto(booking));
            }

            return bookingDtos;
        }

        public async Task<BookingResponseDto> CancelBookingAsync(int bookingId)
        {
            var booking = await _bookingRepository.GetBookingWithDetailsAsync(bookingId);
            if (booking == null)
                return null;

            // Only allow cancellation if status is Requested or Approved
            if (booking.Status != BookingStatus.Requested && booking.Status != BookingStatus.Approved)
                throw new InvalidOperationException("Cannot cancel a booking that is not in Requested or Approved status");

            // Update booking status
            booking.Status = BookingStatus.Cancelled;

            // If payment was completed, set to refunded
            if (booking.PaymentStatus == PaymentStatus.Completed)
            {
                booking.PaymentStatus = PaymentStatus.Refunded;
            }

            // Update vehicle status to Available
            var vehicle = await _vehicleRepository.GetByIdAsync(booking.VehicleId);
            vehicle.Status = VehicleStatus.Available;

            await _vehicleRepository.UpdateAsync(vehicle);
            await _bookingRepository.UpdateAsync(booking);

            return await MapBookingToDto(booking);
        }

        public async Task<PaginatedResponse<BookingResponseDto>> GetPaginatedBookingsAsync(int pageNumber, int pageSize)
        {
            var paginatedBookings = await _bookingRepository.GetPaginatedBookingsAsync(pageNumber, pageSize);

            var bookingDtos = new List<BookingResponseDto>();
            foreach (var booking in paginatedBookings.Items)
            {
                bookingDtos.Add(await MapBookingToDto(booking));
            }

            return new PaginatedResponse<BookingResponseDto>
            {
                Items = bookingDtos,
                TotalCount = paginatedBookings.TotalCount,
                PageNumber = paginatedBookings.PageNumber,
                PageSize = paginatedBookings.PageSize
            };
        }

        private async Task<BookingResponseDto> MapBookingToDto(Booking booking)
        {
            var vehicle = booking.Vehicle ?? await _vehicleRepository.GetByIdAsync(booking.VehicleId);
            var user = booking.User ?? await _userRepository.GetByIdAsync(booking.UserId);

            var vehicleDto = new VehicleDto
            {
                Id = vehicle.Id,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                LicensePlate = vehicle.LicensePlate,
                Color = vehicle.Color,
                DailyRate = vehicle.DailyRate,
                ImageUrl = vehicle.ImageUrl,
                Description = vehicle.Description,
                Seats = vehicle.Seats,
                Transmission = vehicle.Transmission,
                FuelType = vehicle.FuelType,
                Status = vehicle.Status.ToString(),
                CategoryName = vehicle.Category?.Name,
                AverageRating = await _vehicleRepository.GetVehicleAverageRatingAsync(vehicle.Id)
            };

            var userDto = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                Role = user.Role.ToString(),
                RegistrationDate = user.RegistrationDate
            };

            return new BookingResponseDto
            {
                Id = booking.Id,
                BookingDate = booking.BookingDate,
                PickupDate = booking.PickupDate,
                ReturnDate = booking.ReturnDate,
                TotalPrice = booking.TotalPrice,
                Status = booking.Status.ToString(),
                PickupLocation = booking.PickupLocation,
                ReturnLocation = booking.ReturnLocation,
                PaymentMethod = booking.PaymentMethod.ToString(),
                PaymentStatus = booking.PaymentStatus.ToString(),
                Vehicle = vehicleDto,
                User = userDto
            };
        }
    }
}