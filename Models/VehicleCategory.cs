using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.Models
{
    public class VehicleCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        // Navigation properties
        public virtual ICollection<Vehicle> Vehicles { get; set; }
    }
}