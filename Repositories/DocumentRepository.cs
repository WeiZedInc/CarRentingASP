using CarRentalSystem.Data;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Repositories
{
    public interface IDocumentRepository : IRepository<Document>
    {
        Task<List<Document>> GetUserDocumentsAsync(int userId);
        Task<bool> UserHasRequiredDocumentsAsync(int userId);
    }
    public class DocumentRepository : Repository<Document>, IDocumentRepository
    {
        public DocumentRepository(CarRentalDbContext context) : base(context) { }

        public async Task<List<Document>> GetUserDocumentsAsync(int userId)
        {
            return await _context.Documents
                .Where(d => d.UserId == userId)
                .ToListAsync();
        }

        public async Task<bool> UserHasRequiredDocumentsAsync(int userId)
        {
            // Check if the user has both a driver's license and an ID
            var hasDriversLicense = await _context.Documents
                .AnyAsync(d => d.UserId == userId && d.DocumentType == "DriversLicense" && d.IsVerified);

            var hasId = await _context.Documents
                .AnyAsync(d => d.UserId == userId && d.DocumentType == "ID" && d.IsVerified);

            return hasDriversLicense && hasId;
        }
    }


}