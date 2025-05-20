using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class DamageReport
    {
        [Key]
        public int Id { get; set; }

        public string Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal RepairCost { get; set; }

        public bool IsCustomerResponsible { get; set; }

        public DateTime ReportDate { get; set; } = DateTime.UtcNow;

        public string ImageUrl { get; set; }

        // Foreign keys
        public int BookingId { get; set; }

        // Navigation properties
        public virtual Booking Booking { get; set; }
    }
}