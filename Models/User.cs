using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        [Required]
        public UserRole Role { get; set; } = UserRole.Customer;

        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public bool IsEmailVerified { get; set; } = false;

        public string? GoogleId { get; set; }

        // Navigation properties
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<Booking> Bookings { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual LoyaltyProgram LoyaltyProgram { get; set; }
    }
}