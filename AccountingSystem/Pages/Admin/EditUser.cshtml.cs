using AccountingSystem.Data;
using AccountingSystem.Models;
using AccountingSystem.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class EditUserModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public EditUserModel(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [TempData] public string? StatusMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        [BindProperty] public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string CurrentRole { get; set; } = "";

        [BindProperty] public string RoleToAssign { get; set; } = "Accountant";
        [BindProperty] public bool IsActive { get; set; } = true;

        // We store UTC, but html datetime-local uses local time format
        [BindProperty] public DateTime PasswordExpiresAt { get; set; }

        [BindProperty] public DateTime? SuspendedFrom { get; set; }
        [BindProperty] public DateTime? SuspendedUntil { get; set; }

        [BindProperty] public string? SecurityQuestion { get; set; }
        [BindProperty] public string? SecurityAnswerPlain { get; set; }

        [BindProperty] public string? EmailSubject { get; set; }
        [BindProperty] public string? EmailBody { get; set; }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            UserId = user.Id;
            UserName = user.UserName ?? "";
            Email = user.Email ?? "";

            var roles = await _userManager.GetRolesAsync(user);
            CurrentRole = roles.FirstOrDefault() ?? "";
            RoleToAssign = string.IsNullOrWhiteSpace(CurrentRole) ? "Accountant" : CurrentRole;

            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (sec == null)
            {
                // Create default security row if missing
                sec = new UserSecurity
                {
                    UserId = user.Id,
                    IsActive = true,
                    PasswordLastChangedAt = DateTime.UtcNow,
                    PasswordExpiresAt = DateTime.UtcNow.AddDays(90)
                };
                _db.UserSecurities.Add(sec);
                await _db.SaveChangesAsync();
            }

            IsActive = sec.IsActive;
            PasswordExpiresAt = sec.PasswordExpiresAt;
            SuspendedFrom = sec.SuspendedFrom;
            SuspendedUntil = sec.SuspendedUntil;
            SecurityQuestion = sec.SecurityQuestion;

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null) return NotFound();

            // Role update
            var roles = await _userManager.GetRolesAsync(user);
            var current = roles.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(current) && current != RoleToAssign)
                await _userManager.RemoveFromRoleAsync(user, current);

            if (string.IsNullOrWhiteSpace(current) || current != RoleToAssign)
                await _userManager.AddToRoleAsync(user, RoleToAssign);

            // Security update
            var sec = await _db.UserSecurities.FirstAsync(x => x.UserId == user.Id);
            sec.IsActive = IsActive;
            sec.PasswordExpiresAt = PasswordExpiresAt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(PasswordExpiresAt, DateTimeKind.Local).ToUniversalTime()
                : PasswordExpiresAt.ToUniversalTime();

            sec.SuspendedFrom = SuspendedFrom.HasValue
                ? DateTime.SpecifyKind(SuspendedFrom.Value, DateTimeKind.Local).ToUniversalTime()
                : null;

            sec.SuspendedUntil = SuspendedUntil.HasValue
                ? DateTime.SpecifyKind(SuspendedUntil.Value, DateTimeKind.Local).ToUniversalTime()
                : null;

            sec.SecurityQuestion = string.IsNullOrWhiteSpace(SecurityQuestion) ? null : SecurityQuestion.Trim();

            if (!string.IsNullOrWhiteSpace(SecurityAnswerPlain))
            {
                sec.SecurityAnswerHash = SecurityAnswerHasher.Hash(SecurityAnswerPlain);
            }

            await _db.SaveChangesAsync();

            StatusMessage = "User updated.";
            return RedirectToPage(new { id = user.Id });
        }

        public async Task<IActionResult> OnPostUnlockAsync()
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null) return NotFound();

            // Reset lockout
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(-1));
            await _userManager.ResetAccessFailedCountAsync(user);

            StatusMessage = "User unlocked.";
            return RedirectToPage(new { id = user.Id });
        }

        public async Task<IActionResult> OnPostSendEmailAsync()
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                ErrorMessage = "User has no email.";
                return RedirectToPage(new { id = user.Id });
            }

            if (string.IsNullOrWhiteSpace(EmailSubject) || string.IsNullOrWhiteSpace(EmailBody))
            {
                ErrorMessage = "Subject and body are required.";
                return RedirectToPage(new { id = user.Id });
            }

            _db.SentEmails.Add(new SentEmail
            {
                ToEmail = user.Email,
                ToUserId = user.Id,
                Subject = EmailSubject.Trim(),
                BodyHtml = EmailBody.Trim(),
                SentByUserId = _userManager.GetUserId(User),
                SentAtUtc = DateTime.UtcNow,
                Channel = "OutboxDb"
            });

            await _db.SaveChangesAsync();

            StatusMessage = "Email logged (SentEmails table).";
            return RedirectToPage(new { id = user.Id });
        }
    }
}