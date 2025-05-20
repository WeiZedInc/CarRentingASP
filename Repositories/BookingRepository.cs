using CarRentalSystem.Data;
using CarRentalSystem.DTOs;
using CarRentalSystem.Models;
using CarRentingASP.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Repositories
{
    public interface IBookingRepository : IRepository<Booking>
    {
        Task<List<Booking>> GetUserBookingsAsync(int userId);
        Task<Booking> GetBookingWithDetailsAsync(int bookingId);
        Task<List<Booking>> GetPendingBookingsAsync();
        Task<bool> IsVehicleAvailableForDatesAsync(int vehicleId, DateTime startDate, DateTime endDate, int? excludeBookingId = null);
        Task<PaginatedResponse<Booking>> GetPaginatedBookingsAsync(int pageNumber, int pageSize);
    }
    public class BookingRepository : Repository<Booking>, IBookingRepository
    {
        public BookingRepository(CarRentalDbContext context) : base(context) { }

        public async Task<List<Booking>> GetUserBookingsAsync(int userId)
        {
            return await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Vehicle)
                .Include(b => b.DamageReport)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<Booking> GetBookingWithDetailsAsync(int bookingId)
        {
            return await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.User)
                .Include(b => b.DamageReport)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
        }

        public async Task<List<Booking>> GetPendingBookingsAsync()
        {
            return await _context.Bookings
                .Where(b => b.Status == BookingStatus.Requested)
                .Include(b => b.Vehicle)
                .Include(b => b.User)
                .OrderBy(b => b.BookingDate)
                .ToListAsync();
        }

        public async Task<bool> IsVehicleAvailableForDatesAsync(int vehicleId, DateTime startDate, DateTime endDate, int? excludeBookingId = null)
        {
            // Convert dates to UTC if they aren't already
            DateTime startDateUtc = startDate.EnsureUtc();
            DateTime endDateUtc = endDate.EnsureUtc();

            // Check if the vehicle is in maintenance or out of service
            var vehicle = await _context.Vehicles.FindAsync(vehicleId);
            if (vehicle.Status == VehicleStatus.Maintenance || vehicle.Status == VehicleStatus.OutOfService)
                return false;

            // Check for overlapping bookings
            var overlappingBookings = await _context.Bookings
                .Where(b =>
                    b.VehicleId == vehicleId &&
                    (b.Status == BookingStatus.Approved || b.Status == BookingStatus.Requested) &&
                    ((b.PickupDate <= endDateUtc && b.ReturnDate >= startDateUtc)))
                .ToListAsync();

            // If we're updating an existing booking, exclude it from the check
            if (excludeBookingId.HasValue)
            {
                overlappingBookings = overlappingBookings
                    .Where(b => b.Id != excludeBookingId.Value)
                    .ToList();
            }

            return !overlappingBookings.Any();
        }

        public async Task<PaginatedResponse<Booking>> GetPaginatedBookingsAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.Bookings.CountAsync();

            var bookings = await _context.Bookings
                .Include(b => b.Vehicle)
                .Include(b => b.User)
                .OrderByDescending(b => b.BookingDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<Booking>
            {
                Items = bookings,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}