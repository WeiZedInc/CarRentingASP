using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.DTOs
{
    public class UserRegistrationDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MinLength(6)]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }
    }
}