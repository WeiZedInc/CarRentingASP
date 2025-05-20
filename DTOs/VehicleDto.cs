namespace CarRentalSystem.DTOs
{
    public class VehicleDto
    {
        public int Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicensePlate { get; set; }
        public string Color { get; set; }
        public decimal DailyRate { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public int Seats { get; set; }
        public string Transmission { get; set; }
        public string FuelType { get; set; }
        public string Status { get; set; }
        public string CategoryName { get; set; }
        public double AverageRating { get; set; }
    }
}