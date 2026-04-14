#nullable disable
using System.ComponentModel.DataAnnotations;
using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IPasswordHasher<IdentityUser> _hasher;

        public ResetPasswordModel(
            UserManager<IdentityUser> userManager,
            ApplicationDbContext db,
            IPasswordHasher<IdentityUser> hasher)
        {
            _userManager = userManager;
            _db = db;
            _hasher = hasher;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [TempData] public string StatusMessage { get; set; }
        [TempData] public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            public string UserId { get; set; }

            [Required, EmailAddress]
            public string Email { get; set; }

            [Required]
            public string Code { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet(string userId, string email, string code)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
                return BadRequest("Missing reset information.");

            Input.UserId = userId;
            Input.Email = email;
            Input.Code = code;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            StatusMessage = null;
            ErrorMessage = null;

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByIdAsync(Input.UserId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email) ||
                !string.Equals(user.Email, Input.Email, StringComparison.OrdinalIgnoreCase))
            {
                ErrorMessage = "Invalid reset attempt.";
                return Page();
            }

            // Prevent password reuse (Sprint #11)
            var history = await _db.PasswordHistories
                .AsNoTracking()
                .Where(h => h.UserId == user.Id)
                .OrderByDescending(h => h.CreatedAt)
                .Take(10)
                .ToListAsync();

            foreach (var h in history)
            {
                var check = _hasher.VerifyHashedPassword(user, h.PasswordHash, Input.Password);
                if (check == PasswordVerificationResult.Success)
                {
                    ModelState.AddModelError(string.Empty, "You cannot reuse a previous password.");
                    return Page();
                }
            }

            // Reset password (Identity runs your validators too, including your StartsWithLetter validator)
            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                return Page();
            }

            // Store new password hash in history (after reset, user.PasswordHash is updated)
            user = await _userManager.FindByIdAsync(user.Id); // re-fetch to ensure PasswordHash updated

            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                _db.PasswordHistories.Add(new PasswordHistory
                {
                    UserId = user.Id,
                    PasswordHash = user.PasswordHash,
                    CreatedAt = DateTime.UtcNow
                });
            }

            // Update UserSecurity fields (Sprint #14-15)
            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (sec == null)
            {
                sec = new UserSecurity
                {
                    UserId = user.Id
                };
                _db.UserSecurities.Add(sec);
            }

            sec.PasswordLastChangedAt = DateTime.UtcNow;
            sec.PasswordExpiresAt = DateTime.UtcNow.AddDays(90);

            // Optional: unlock user after reset
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddMinutes(-1));
            await _userManager.ResetAccessFailedCountAsync(user);

            await _db.SaveChangesAsync();

            StatusMessage = "Password reset successful. You can now log in.";
            return RedirectToPage("./Login");
        }
    }
}