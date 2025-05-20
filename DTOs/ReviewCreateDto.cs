using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.DTOs
{
    public class ReviewCreateDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        public string Comment { get; set; }
    }
}