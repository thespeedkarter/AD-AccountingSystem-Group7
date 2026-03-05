using AccountingSystem.Data;
using AccountingSystem.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AccountingSystem.Pages.Account
{
    public class ForgotPasswordCustomModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public ForgotPasswordCustomModel(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public string? Message { get; set; }
        public string SecurityQuestion { get; set; } = "";

        [BindProperty, Required]
        public string Username { get; set; } = "";

        [BindProperty, Required, EmailAddress]
        public string Email { get; set; } = "";

        [BindProperty, Required]
        public string SecurityAnswer { get; set; } = "";

        [BindProperty, Required]
        public string NewPassword { get; set; } = "";

        [BindProperty, Required, Compare(nameof(NewPassword))]
        public string ConfirmPassword { get; set; } = "";

        public async Task OnGetAsync()
        {
            // blank
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByNameAsync(Username.Trim());
            if (user == null || string.IsNullOrWhiteSpace(user.Email) || !user.Email.Equals(Email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                Message = "User not found or email does not match.";
                return Page();
            }

            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (sec == null || string.IsNullOrWhiteSpace(sec.SecurityQuestion) || string.IsNullOrWhiteSpace(sec.SecurityAnswerHash))
            {
                SecurityQuestion = sec?.SecurityQuestion ?? "";
                Message = "Security question is not set. Ask an administrator to set it.";
                return Page();
            }

            SecurityQuestion = sec.SecurityQuestion;

            var providedHash = SecurityAnswerHasher.Hash(SecurityAnswer);
            if (!string.Equals(providedHash, sec.SecurityAnswerHash, StringComparison.OrdinalIgnoreCase))
            {
                Message = "Security answer incorrect.";
                return Page();
            }

            // Reset password using Identity token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, NewPassword);

            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return Page();
            }

            // Update security dates
            sec.PasswordLastChangedAt = DateTime.UtcNow;
            sec.PasswordExpiresAt = DateTime.UtcNow.AddDays(90);
            await _db.SaveChangesAsync();

            Message = "Password reset successfully. You can login now.";
            return Page();
        }
    }
}