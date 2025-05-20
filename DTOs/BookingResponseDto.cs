namespace CarRentalSystem.DTOs
{
    public class BookingResponseDto
    {
        public int Id { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime PickupDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; }
        public string PickupLocation { get; set; }
        public string ReturnLocation { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public VehicleDto Vehicle { get; set; }
        public UserResponseDto User { get; set; }
    }
}