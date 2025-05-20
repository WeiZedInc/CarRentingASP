using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.DTOs
{
    public class DamageReportCreateDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required, Range(0, 100000)]
        public decimal RepairCost { get; set; }

        public bool IsCustomerResponsible { get; set; }

        public string ImageUrl { get; set; }
    }
}