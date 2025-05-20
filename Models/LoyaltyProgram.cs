using System.ComponentModel.DataAnnotations;

namespace CarRentalSystem.Models
{
    public class LoyaltyProgram
    {
        [Key]
        public int Id { get; set; }

        public int Points { get; set; } = 0;

        public string Tier { get; set; } = "Bronze";  // Bronze, Silver, Gold, Platinum

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        // Foreign keys
        public int UserId { get; set; }

        // Navigation properties
        public virtual User User { get; set; }
        public virtual ICollection<LoyaltyTransaction> Transactions { get; set; }
    }
}