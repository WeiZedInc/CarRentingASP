using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.Models
{
    public class Document
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DocumentType { get; set; }  // DriversLicense, ID

        public string DocumentNumber { get; set; }

        [Required]
        public string FileUrl { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedDate { get; set; }

        // Foreign keys
        public int UserId { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
    }
}