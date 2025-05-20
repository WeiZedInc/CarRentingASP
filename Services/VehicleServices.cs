using CarRentalSystem.DTOs;
using CarRentalSystem.Models;
using CarRentalSystem.Repositories;

namespace CarRentalSystem.Services
{
    public interface IVehicleService
    {
        Task<List<VehicleDto>> GetAllVehiclesAsync();
        Task<VehicleDto> GetVehicleByIdAsync(int id);
        Task<List<VehicleDto>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate);
        Task<VehicleDto> CreateVehicleAsync(VehicleCreateDto vehicleDto);
        Task<VehicleDto> UpdateVehicleAsync(int id, VehicleUpdateDto vehicleDto);
        Task DeleteVehicleAsync(int id);
        Task<List<VehicleDto>> GetVehiclesByCategoryAsync(int categoryId);
        Task<List<ReviewResponseDto>> GetVehicleReviewsAsync(int vehicleId);
        Task<List<VehicleDto>> SearchVehiclesAsync(string searchTerm);
        Task<PaginatedResponse<VehicleDto>> GetPaginatedVehiclesAsync(int pageNumber, int pageSize);
    }

    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IReviewRepository _reviewRepository;

        public VehicleService(
            IVehicleRepository vehicleRepository,
            ICategoryRepository categoryRepository,
            IReviewRepository reviewRepository)
        {
            _vehicleRepository = vehicleRepository;
            _categoryRepository = categoryRepository;
            _reviewRepository = reviewRepository;
        }

        public async Task<List<VehicleDto>> GetAllVehiclesAsync()
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            return await MapVehiclesToDtos(vehicles);
        }

        public async Task<VehicleDto> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _vehicleRepository.GetVehicleWithDetailsAsync(id);
            if (vehicle == null)
                return null;

            return await MapVehicleToDto(vehicle);
        }

        public async Task<List<VehicleDto>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate)
        {
            var vehicles = await _vehicleRepository.GetAvailableVehiclesAsync(startDate, endDate);
            return await MapVehiclesToDtos(vehicles);
        }

        public async Task<VehicleDto> CreateVehicleAsync(VehicleCreateDto vehicleDto)
        {
            var category = await _categoryRepository.GetByIdAsync(vehicleDto.CategoryId);
            if (category == null)
                throw new ArgumentException("Invalid category ID");

            var vehicle = new Vehicle
            {
                Make = vehicleDto.Make,
                Model = vehicleDto.Model,
                Year = vehicleDto.Year,
                LicensePlate = vehicleDto.LicensePlate,
                VIN = vehicleDto.VIN,
                Color = vehicleDto.Color,
                Mileage = vehicleDto.Mileage,
                DailyRate = vehicleDto.DailyRate,
                ImageUrl = vehicleDto.ImageUrl,
                Description = vehicleDto.Description,
                Seats = vehicleDto.Seats,
                Transmission = vehicleDto.Transmission,
                FuelType = vehicleDto.FuelType,
                Status = VehicleStatus.Available,
                CategoryId = vehicleDto.CategoryId
            };

            var createdVehicle = await _vehicleRepository.AddAsync(vehicle);
            return await MapVehicleToDto(createdVehicle);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(int id, VehicleUpdateDto vehicleDto)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
                return null;

            var category = await _categoryRepository.GetByIdAsync(vehicleDto.CategoryId);
            if (category == null)
                throw new ArgumentException("Invalid category ID");

            // Update properties
            vehicle.Make = vehicleDto.Make;
            vehicle.Model = vehicleDto.Model;
            vehicle.Year = vehicleDto.Year;
            vehicle.LicensePlate = vehicleDto.LicensePlate;
            vehicle.VIN = vehicleDto.VIN;
            vehicle.Color = vehicleDto.Color;
            vehicle.Mileage = vehicleDto.Mileage;
            vehicle.DailyRate = vehicleDto.DailyRate;
            vehicle.ImageUrl = vehicleDto.ImageUrl;
            vehicle.Description = vehicleDto.Description;
            vehicle.Seats = vehicleDto.Seats;
            vehicle.Transmission = vehicleDto.Transmission;
            vehicle.FuelType = vehicleDto.FuelType;
            vehicle.CategoryId = vehicleDto.CategoryId;

            // If status is provided, update it
            if (!string.IsNullOrEmpty(vehicleDto.Status))
            {
                if (Enum.TryParse<VehicleStatus>(vehicleDto.Status, out var status))
                {
                    vehicle.Status = status;
                }
            }

            await _vehicleRepository.UpdateAsync(vehicle);

            var updatedVehicle = await _vehicleRepository.GetVehicleWithDetailsAsync(id);
            return await MapVehicleToDto(updatedVehicle);
        }

        public async Task DeleteVehicleAsync(int id)
        {
            await _vehicleRepository.DeleteAsync(id);
        }

        public async Task<List<VehicleDto>> GetVehiclesByCategoryAsync(int categoryId)
        {
            var vehicles = await _vehicleRepository.GetVehiclesByCategoryAsync(categoryId);
            return await MapVehiclesToDtos(vehicles);
        }

        public async Task<List<ReviewResponseDto>> GetVehicleReviewsAsync(int vehicleId)
        {
            var reviews = await _reviewRepository.GetVehicleReviewsAsync(vehicleId);

            return reviews.Select(r => new ReviewResponseDto
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment,
                ReviewDate = r.ReviewDate,
                UserName = $"{r.User.FirstName} {r.User.LastName}"
            }).ToList();
        }

        public async Task<List<VehicleDto>> SearchVehiclesAsync(string searchTerm)
        {
            var vehicles = await _vehicleRepository.SearchVehiclesAsync(searchTerm);
            return await MapVehiclesToDtos(vehicles);
        }

        public async Task<PaginatedResponse<VehicleDto>> GetPaginatedVehiclesAsync(int pageNumber, int pageSize)
        {
            var paginatedVehicles = await _vehicleRepository.GetPaginatedVehiclesAsync(pageNumber, pageSize);

            var vehicleDtos = await MapVehiclesToDtos(paginatedVehicles.Items);

            return new PaginatedResponse<VehicleDto>
            {
                Items = vehicleDtos,
                TotalCount = paginatedVehicles.TotalCount,
                PageNumber = paginatedVehicles.PageNumber,
                PageSize = paginatedVehicles.PageSize
            };
        }

        private async Task<VehicleDto> MapVehicleToDto(Vehicle vehicle)
        {
            var averageRating = await _vehicleRepository.GetVehicleAverageRatingAsync(vehicle.Id);

            return new VehicleDto
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
                AverageRating = averageRating
            };
        }

        private async Task<List<VehicleDto>> MapVehiclesToDtos(List<Vehicle> vehicles)
        {
            var vehicleDtos = new List<VehicleDto>();

            foreach (var vehicle in vehicles)
            {
                vehicleDtos.Add(await MapVehicleToDto(vehicle));
            }

            return vehicleDtos;
        }
    }
}