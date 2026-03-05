using System;
using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public class SentEmail
    {
        public int SentEmailId { get; set; }

        [Required, StringLength(256)]
        public string ToEmail { get; set; } = string.Empty;

        [StringLength(256)]
        public string? ToUserId { get; set; }

        [Required, StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string BodyHtml { get; set; } = string.Empty;

        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

        [StringLength(450)]
        public string? SentByUserId { get; set; }

        [StringLength(50)]
        public string Channel { get; set; } = "OutboxDb"; // “smtp”, etc later if you want
    }
}