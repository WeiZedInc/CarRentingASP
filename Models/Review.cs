using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.Models
{
    public class Review
    {
        [Key]
        public int Id { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int UserId { get; set; }
        public int VehicleId { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual Vehicle Vehicle { get; set; }
    }
}