using System;
using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public enum NormalSide { Debit = 0, Credit = 1 }

    public enum AccountCategory
    {
        Asset = 0,
        Liability = 1,
        Equity = 2,
        Revenue = 3,
        Expense = 4
    }

    public class ChartAccount
    {
        public int ChartAccountId { get; set; }

        [Required, StringLength(120)]
        public string AccountName { get; set; } = string.Empty;

        [Required]
        public int AccountNumber { get; set; } // no decimals/alphanumeric

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public NormalSide NormalSide { get; set; }

        [Required]
        public AccountCategory Category { get; set; }

        [StringLength(100)]
        public string? Subcategory { get; set; }

        [Required]
        [Range(0, 999999999999.99)]
        public decimal InitialBalance { get; set; } = 0m;

        [Required]
        public decimal Debit { get; set; } = 0m;

        [Required]
        public decimal Credit { get; set; } = 0m;

        [Required]
        public decimal Balance { get; set; } = 0m;

        public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? AddedByUserId { get; set; }

        [Required]
        [StringLength(10)]
        public string OrderCode { get; set; } = "01"; // “cash can be 01”

        [Required]
        [StringLength(5)]
        public string Statement { get; set; } = "BS"; // IS, BS, RE

        [StringLength(500)]
        public string? Comment { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
