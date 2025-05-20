using CarRentalSystem.Data;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Repositories
{
    public interface IReviewRepository : IRepository<Review>
    {
        Task<List<Review>> GetVehicleReviewsAsync(int vehicleId);
        Task<List<Review>> GetUserReviewsAsync(int userId);
        Task<bool> HasUserReviewedVehicleAsync(int userId, int vehicleId);
    }
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(CarRentalDbContext context) : base(context) { }

        public async Task<List<Review>> GetVehicleReviewsAsync(int vehicleId)
        {
            return await _context.Reviews
                .Where(r => r.VehicleId == vehicleId)
                .Include(r => r.User)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }

        public async Task<List<Review>> GetUserReviewsAsync(int userId)
        {
            return await _context.Reviews
                .Where(r => r.UserId == userId)
                .Include(r => r.Vehicle)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();
        }

        public async Task<bool> HasUserReviewedVehicleAsync(int userId, int vehicleId)
        {
            return await _context.Reviews
                .AnyAsync(r => r.UserId == userId && r.VehicleId == vehicleId);
        }
    }


}