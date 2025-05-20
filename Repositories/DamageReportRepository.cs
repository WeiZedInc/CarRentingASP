using CarRentalSystem.Data;
using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Repositories
{
    public interface IDamageReportRepository : IRepository<DamageReport>
    {
        Task<DamageReport> GetDamageReportByBookingIdAsync(int bookingId);
    }
    public class DamageReportRepository : Repository<DamageReport>, IDamageReportRepository
    {
        public DamageReportRepository(CarRentalDbContext context) : base(context) { }

        public async Task<DamageReport> GetDamageReportByBookingIdAsync(int bookingId)
        {
            return await _context.DamageReports
                .FirstOrDefaultAsync(dr => dr.BookingId == bookingId);
        }
    }


}