using System;
using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public class LedgerEntry
    {
        public int LedgerEntryId { get; set; }

        [Required]
        public int ChartAccountId { get; set; }
        public ChartAccount? ChartAccount { get; set; }

        [Required]
        public DateTime EntryDate { get; set; }

        // PR = post reference (Sprint 3 #14-16)
        [Required]
        public int JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }

        [StringLength(200)]
        public string? Description { get; set; } // usually empty

        [Required]
        public decimal Debit { get; set; } = 0m;

        [Required]
        public decimal Credit { get; set; } = 0m;

        // Running balance AFTER this entry
        [Required]
        public decimal BalanceAfter { get; set; } = 0m;

        public DateTime PostedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
