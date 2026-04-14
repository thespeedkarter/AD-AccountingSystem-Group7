using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public enum JournalStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Posted = 3
    }

    public class JournalEntry
    {
        public int JournalEntryId { get; set; }

        [Required]
        public DateTime EntryDate { get; set; } = DateTime.Today;

        [StringLength(200)]
        public string? Description { get; set; }

        // Sprint 4: Adjusting entry flag
        public bool IsAdjusting { get; set; } = false;

        public JournalStatus Status { get; set; } = JournalStatus.Pending;

        [StringLength(500)]
        public string? ManagerComment { get; set; }

        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }

        [StringLength(450)]
        public string? PostedByUserId { get; set; }
        public DateTime? PostedAtUtc { get; set; }

        public List<JournalLine> Lines { get; set; } = new();
        public List<JournalAttachment> Attachments { get; set; } = new();
    }
}