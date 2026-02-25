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
        public string? Description { get; set; } // usually empty per sprint

        public JournalStatus Status { get; set; } = JournalStatus.Pending;

        // Manager rejection reason/comment (Sprint 3 #5)
        [StringLength(500)]
        public string? ManagerComment { get; set; }

        // Who created it
        [StringLength(450)]
        public string? CreatedByUserId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Approval
        [StringLength(450)]
        public string? ApprovedByUserId { get; set; }
        public DateTime? ApprovedAtUtc { get; set; }

        // Posting
        [StringLength(450)]
        public string? PostedByUserId { get; set; }
        public DateTime? PostedAtUtc { get; set; }

        public List<JournalLine> Lines { get; set; } = new();
        public List<JournalAttachment> Attachments { get; set; } = new();
    }
}
