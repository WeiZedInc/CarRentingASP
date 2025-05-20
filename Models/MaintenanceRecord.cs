using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarRentalSystem.Models
{
    public class MaintenanceRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MaintenanceType { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }

        public string Description { get; set; }

        public DateTime MaintenanceDate { get; set; } = DateTime.UtcNow;

        public DateTime? NextMaintenanceDate { get; set; }

        // Foreign keys
        public int VehicleId { get; set; }

        // Navigation properties
        public virtual Vehicle Vehicle { get; set; }
    }
}