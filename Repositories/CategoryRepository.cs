using CarRentalSystem.Data;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Repositories
{
    public interface ICategoryRepository : IRepository<VehicleCategory>
    {
        Task<bool> IsCategoryNameUniqueAsync(string name);
    }
    public class CategoryRepository : Repository<VehicleCategory>, ICategoryRepository
    {
        public CategoryRepository(CarRentalDbContext context) : base(context) { }

        public async Task<bool> IsCategoryNameUniqueAsync(string name)
        {
            return !await _context.VehicleCategories.AnyAsync(c => c.Name == name);
        }
    }


}