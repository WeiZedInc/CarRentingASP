using CarRentalSystem.Data;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User> GetUserWithDetailsAsync(int userId);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<List<User>> GetUsersByRoleAsync(UserRole role);
    }
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(CarRentalDbContext context) : base(context) { }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> GetUserWithDetailsAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Documents)
                .Include(u => u.LoyaltyProgram)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<bool> IsEmailUniqueAsync(string email)
        {
            return !await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<List<User>> GetUsersByRoleAsync(UserRole role)
        {
            return await _context.Users
                .Where(u => u.Role == role)
                .ToListAsync();
        }
    }
}