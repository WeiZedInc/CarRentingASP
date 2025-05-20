using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.DTOs
{
    public class DocumentCreateDto
    {
        [Required]
        public string DocumentType { get; set; }

        public string DocumentNumber { get; set; }

        [Required]
        public string FileUrl { get; set; }
    }
}