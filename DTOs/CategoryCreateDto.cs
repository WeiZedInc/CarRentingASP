using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.DTOs
{
    public class CategoryCreateDto
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}