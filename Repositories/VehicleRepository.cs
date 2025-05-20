using CarRentalSystem.Data;
using CarRentalSystem.DTOs;
using CarRentalSystem.Models;
using CarRentingASP.Extensions;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Repositories
{
    public interface IVehicleRepository : IRepository<Vehicle>
    {
        Task<List<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate);
        Task<Vehicle> GetVehicleWithDetailsAsync(int vehicleId);
        Task<List<Vehicle>> GetVehiclesByCategoryAsync(int categoryId);
        Task<double> GetVehicleAverageRatingAsync(int vehicleId);
        Task<List<Vehicle>> SearchVehiclesAsync(string searchTerm);
        Task<PaginatedResponse<Vehicle>> GetPaginatedVehiclesAsync(int pageNumber, int pageSize);
    }
    public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(CarRentalDbContext context) : base(context) { }

        public async Task<List<Vehicle>> GetAvailableVehiclesAsync(DateTime startDate, DateTime endDate)
        {
            // Convert dates to UTC if they aren't already
            DateTime startDateUtc = startDate.EnsureUtc();
            DateTime endDateUtc = endDate.EnsureUtc();

            // Get all vehicles that are available (not in maintenance or out of service)
            var availableVehicles = _context.Vehicles
                .Where(v => v.Status != VehicleStatus.Maintenance && v.Status != VehicleStatus.OutOfService)
                .Include(v => v.Category);

            // Get all vehicles that have overlapping bookings with the requested dates
            var bookedVehicleIds = await _context.Bookings
                .Where(b =>
                    (b.Status == BookingStatus.Approved || b.Status == BookingStatus.Requested) &&
                    ((b.PickupDate <= endDateUtc && b.ReturnDate >= startDateUtc)))
                .Select(b => b.VehicleId)
                .Distinct()
                .ToListAsync();

            // Filter out vehicles with overlapping bookings
            var result = await availableVehicles
                .Where(v => !bookedVehicleIds.Contains(v.Id))
                .ToListAsync();

            return result;
        }

        public async Task<Vehicle> GetVehicleWithDetailsAsync(int vehicleId)
        {
            return await _context.Vehicles
                .Include(v => v.Category)
                .Include(v => v.MaintenanceRecords)
                .Include(v => v.Reviews)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
        }

        public async Task<List<Vehicle>> GetVehiclesByCategoryAsync(int categoryId)
        {
            return await _context.Vehicles
                .Where(v => v.CategoryId == categoryId)
                .Include(v => v.Category)
                .ToListAsync();
        }

        public async Task<double> GetVehicleAverageRatingAsync(int vehicleId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.VehicleId == vehicleId)
                .ToListAsync();

            if (reviews.Count == 0)
                return 0;

            return reviews.Average(r => r.Rating);
        }

        public async Task<List<Vehicle>> SearchVehiclesAsync(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return await GetAllAsync();

            return await _context.Vehicles
                .Where(v =>
                    v.Make.Contains(searchTerm) ||
                    v.Model.Contains(searchTerm) ||
                    v.Category.Name.Contains(searchTerm) ||
                    v.Description.Contains(searchTerm))
                .Include(v => v.Category)
                .ToListAsync();
        }

        public async Task<PaginatedResponse<Vehicle>> GetPaginatedVehiclesAsync(int pageNumber, int pageSize)
        {
            var totalCount = await _context.Vehicles.CountAsync();

            var vehicles = await _context.Vehicles
                .Include(v => v.Category)
                .OrderBy(v => v.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResponse<Vehicle>
            {
                Items = vehicles,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}