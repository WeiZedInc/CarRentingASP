using CarRentalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace CarRentalSystem.Data
{
    public class CarRentalDbContext : DbContext
    {
        public CarRentalDbContext(DbContextOptions<CarRentalDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<VehicleCategory> VehicleCategories { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<LoyaltyProgram> LoyaltyPrograms { get; set; }
        public DbSet<LoyaltyTransaction> LoyaltyTransactions { get; set; }
        public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }
        public DbSet<DamageReport> DamageReports { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure relationships and constraints

            // User to Documents - One to Many
            modelBuilder.Entity<User>()
                .HasMany(u => u.Documents)
                .WithOne(d => d.User)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User to Bookings - One to Many
            modelBuilder.Entity<User>()
                .HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // User to Reviews - One to Many
            modelBuilder.Entity<User>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User to LoyaltyProgram - One to One
            modelBuilder.Entity<User>()
                .HasOne(u => u.LoyaltyProgram)
                .WithOne(lp => lp.User)
                .HasForeignKey<LoyaltyProgram>(lp => lp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vehicle to Category - Many to One
            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Category)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vehicle to Bookings - One to Many
            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Bookings)
                .WithOne(b => b.Vehicle)
                .HasForeignKey(b => b.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Vehicle to Reviews - One to Many
            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.Reviews)
                .WithOne(r => r.Vehicle)
                .HasForeignKey(r => r.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Vehicle to MaintenanceRecords - One to Many
            modelBuilder.Entity<Vehicle>()
                .HasMany(v => v.MaintenanceRecords)
                .WithOne(mr => mr.Vehicle)
                .HasForeignKey(mr => mr.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking to DamageReport - One to One
            modelBuilder.Entity<Booking>()
                .HasOne(b => b.DamageReport)
                .WithOne(dr => dr.Booking)
                .HasForeignKey<DamageReport>(dr => dr.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // LoyaltyProgram to LoyaltyTransactions - One to Many
            modelBuilder.Entity<LoyaltyProgram>()
                .HasMany(lp => lp.Transactions)
                .WithOne(lt => lt.LoyaltyProgram)
                .HasForeignKey(lt => lt.LoyaltyProgramId)
                .OnDelete(DeleteBehavior.Cascade);

            // User to Notifications - One to Many
            modelBuilder.Entity<User>()
                .HasMany<Notification>()
                .WithOne(n => n.User)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Add unique constraints
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.LicensePlate)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => v.VIN)
                .IsUnique();
        }
    }
}