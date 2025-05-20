namespace CarRentalSystem.DTOs
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Role { get; set; }
        public DateTime RegistrationDate { get; set; }
        public int LoyaltyPoints { get; set; }
        public string LoyaltyTier { get; set; }
    }
}