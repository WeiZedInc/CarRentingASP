namespace CarRentalSystem.DTOs
{
    public class DocumentResponseDto
    {
        public int Id { get; set; }
        public string DocumentType { get; set; }
        public string DocumentNumber { get; set; }
        public string FileUrl { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsVerified { get; set; }
        public DateTime? VerifiedDate { get; set; }
    }
}