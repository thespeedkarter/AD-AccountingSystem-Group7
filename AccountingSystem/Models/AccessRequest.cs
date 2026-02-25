using System;
using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public enum AccessRequestStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2
    }

    public class AccessRequest
    {
        public int AccessRequestId { get; set; }

        [Required, StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [Required]
        public DateTime DateOfBirth { get; set; }

        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public AccessRequestStatus Status { get; set; } = AccessRequestStatus.Pending;

        [StringLength(500)]
        public string? AdminComment { get; set; }

        // Identity user id of admin who processed it (nullable until processed)
        [StringLength(450)]
        public string? ProcessedByUserId { get; set; }

        public DateTime? ProcessedAt { get; set; }

        // ✅ NEW: store generated username for the approved user
        [StringLength(256)]
        public string? ApprovedUsername { get; set; }

        // ✅ NEW: store reset link so user can set their password
        [StringLength(2000)]
        public string? SetPasswordLink { get; set; }
    }
}
