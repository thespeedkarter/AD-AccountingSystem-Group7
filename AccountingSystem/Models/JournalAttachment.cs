using System;
using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public class JournalAttachment
    {
        public int JournalAttachmentId { get; set; }

        public int JournalEntryId { get; set; }
        public JournalEntry? JournalEntry { get; set; }

        [Required, StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required, StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? UploadedByUserId { get; set; }
    }
}
