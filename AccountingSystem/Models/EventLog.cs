using System;
using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public class EventLog
    {
        public int EventLogId { get; set; }

        [Required, StringLength(100)]
        public string TableName { get; set; } = string.Empty;

        [Required]
        public int RecordId { get; set; }

        [Required]
        public int Action { get; set; }  // INT in DB

        public string? BeforeJson { get; set; } // nvarchar(max)
        public string? AfterJson { get; set; }  // nvarchar(max)

        [StringLength(450)]
        public string? UserId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}