using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.Models
{
    public class LoyaltyTransaction
    {
        [Key]
        public int Id { get; set; }

        public int Points { get; set; }

        [Required]
        public string TransactionType { get; set; }  // Earned, Redeemed

        public string Description { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int LoyaltyProgramId { get; set; }

        // Navigation properties
        public virtual LoyaltyProgram LoyaltyProgram { get; set; }
    }
}