using CarRentalSystem.Data;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Repositories
{
    public interface ILoyaltyProgramRepository : IRepository<LoyaltyProgram>
    {
        Task<LoyaltyProgram> GetUserLoyaltyProgramAsync(int userId);
        Task AddLoyaltyPointsAsync(int userId, int points, string description);
        Task<List<LoyaltyTransaction>> GetUserLoyaltyTransactionsAsync(int userId);
    }
    public class LoyaltyProgramRepository : Repository<LoyaltyProgram>, ILoyaltyProgramRepository
    {
        public LoyaltyProgramRepository(CarRentalDbContext context) : base(context) { }

        public async Task<LoyaltyProgram> GetUserLoyaltyProgramAsync(int userId)
        {
            return await _context.LoyaltyPrograms
                .Include(lp => lp.Transactions)
                .FirstOrDefaultAsync(lp => lp.UserId == userId);
        }

        public async Task AddLoyaltyPointsAsync(int userId, int points, string description)
        {
            var loyaltyProgram = await GetUserLoyaltyProgramAsync(userId);

            if (loyaltyProgram == null)
            {
                loyaltyProgram = new LoyaltyProgram
                {
                    UserId = userId,
                    Points = 0,
                    Tier = "Bronze",
                    LastUpdated = DateTime.UtcNow
                };

                await _context.LoyaltyPrograms.AddAsync(loyaltyProgram);
                await _context.SaveChangesAsync();
            }

            loyaltyProgram.Points += points;

            // Update tier based on total points
            if (loyaltyProgram.Points >= 5000)
                loyaltyProgram.Tier = "Platinum";
            else if (loyaltyProgram.Points >= 2000)
                loyaltyProgram.Tier = "Gold";
            else if (loyaltyProgram.Points >= 1000)
                loyaltyProgram.Tier = "Silver";
            else
                loyaltyProgram.Tier = "Bronze";

            loyaltyProgram.LastUpdated = DateTime.UtcNow;

            var transaction = new LoyaltyTransaction
            {
                LoyaltyProgramId = loyaltyProgram.Id,
                Points = points,
                TransactionType = points >= 0 ? "Earned" : "Redeemed",
                Description = description,
                TransactionDate = DateTime.UtcNow
            };

            await _context.LoyaltyTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<List<LoyaltyTransaction>> GetUserLoyaltyTransactionsAsync(int userId)
        {
            var loyaltyProgram = await GetUserLoyaltyProgramAsync(userId);

            if (loyaltyProgram == null)
                return new List<LoyaltyTransaction>();

            return await _context.LoyaltyTransactions
                .Where(lt => lt.LoyaltyProgramId == loyaltyProgram.Id)
                .OrderByDescending(lt => lt.TransactionDate)
                .ToListAsync();
        }
    }
}