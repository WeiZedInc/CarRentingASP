using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class Vehicle
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Make { get; set; }

        [Required]
        public string Model { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public string LicensePlate { get; set; }

        [Required]
        public string VIN { get; set; }

        public string Color { get; set; }

        public int Mileage { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal DailyRate { get; set; }

        public string ImageUrl { get; set; }

        public string Description { get; set; }

        public int Seats { get; set; }

        public string Transmission { get; set; }  // Automatic, Manual

        public string FuelType { get; set; }      // Gasoline, Diesel, Electric, Hybrid

        [Required]
        public VehicleStatus Status { get; set; } = VehicleStatus.Available;

        // Foreign keys
        public int CategoryId { get; set; }

        // Navigation properties
        public virtual VehicleCategory Category { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<MaintenanceRecord> MaintenanceRecords { get; set; }
    }
}