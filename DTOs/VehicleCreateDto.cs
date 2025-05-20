using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.DTOs
{
    public class VehicleCreateDto
    {
        [Required]
        public string Make { get; set; }

        [Required]
        public string Model { get; set; }

        [Required, Range(1900, 2100)]
        public int Year { get; set; }

        [Required]
        public string LicensePlate { get; set; }

        [Required]
        public string VIN { get; set; }

        public string Color { get; set; }

        [Range(0, int.MaxValue)]
        public int Mileage { get; set; }

        [Required, Range(0.01, 10000)]
        public decimal DailyRate { get; set; }

        public string ImageUrl { get; set; }

        public string Description { get; set; }

        [Range(1, 20)]
        public int Seats { get; set; }

        public string Transmission { get; set; }

        public string FuelType { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }
}