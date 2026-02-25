using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public class PasswordHistory
    {
        public int PasswordHistoryId { get; set; }

        [Required, StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
