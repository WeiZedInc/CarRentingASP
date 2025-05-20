namespace CarRentalSystem.DTOs
{
    public class BookingUpdateDto
    {
        public DateTime? PickupDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string PickupLocation { get; set; }
        public string ReturnLocation { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
    }
}