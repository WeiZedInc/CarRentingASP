namespace CarRentalSystem.DTOs
{
    public class DamageReportResponseDto
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public decimal RepairCost { get; set; }
        public bool IsCustomerResponsible { get; set; }
        public DateTime ReportDate { get; set; }
        public string ImageUrl { get; set; }
        public int BookingId { get; set; }
    }
}