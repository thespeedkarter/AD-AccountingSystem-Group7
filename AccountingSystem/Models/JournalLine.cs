using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public class JournalLine
    {
        public int JournalLineId { get; set; }

        public int JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }

        // Must be an account from Chart of Accounts
        [Required]
        public int ChartAccountId { get; set; }
        public ChartAccount? ChartAccount { get; set; }

        // Debits come before credits in UI (we’ll enforce later)
        [Required]
        [Range(0, 999999999999.99)]
        public decimal Debit { get; set; } = 0m;

        [Required]
        [Range(0, 999999999999.99)]
        public decimal Credit { get; set; } = 0m;

        [StringLength(200)]
        public string? Memo { get; set; } // optional line memo
    }
}
