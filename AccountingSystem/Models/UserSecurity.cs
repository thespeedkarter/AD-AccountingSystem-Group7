using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Models
{
    public class UserSecurity
    {
        [Key, StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        public DateTime PasswordLastChangedAt { get; set; } = DateTime.UtcNow;

        public DateTime PasswordExpiresAt { get; set; } = DateTime.UtcNow.AddDays(90);

        // For Sprint 1 #17: suspension start->end
        public DateTime? SuspendedFrom { get; set; }
        public DateTime? SuspendedUntil { get; set; }

        // Convenience: active flag
        public bool IsActive { get; set; } = true;

        [StringLength(200)]
        public string? SecurityQuestion { get; set; }

        // Store hash
        [StringLength(128)]
        public string? SecurityAnswerHash { get; set; }
    }
}
