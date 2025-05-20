using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.DTOs
{
    public class BookingCreateDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public DateTime PickupDate { get; set; }

        [Required]
        public DateTime ReturnDate { get; set; }

        public string PickupLocation { get; set; }

        public string ReturnLocation { get; set; }

        [Required]
        public string PaymentMethod { get; set; }
    }
}