using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        public DateTime BookingDate { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        [Required]
        public BookingStatus Status { get; set; } = BookingStatus.Requested;

        public string PickupLocation { get; set; }

        public string ReturnLocation { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;

        public string Notes { get; set; }

        // Foreign keys
        public int UserId { get; set; }
        public int VehicleId { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual DamageReport DamageReport { get; set; }
    }
}