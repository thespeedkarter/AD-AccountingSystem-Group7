#nullable disable
using System.ComponentModel.DataAnnotations;
using AccountingSystem.Data;
using AccountingSystem.Models;
using AccountingSystem.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public ForgotPasswordModel(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string SecurityQuestion { get; set; }
        public string ResetLink { get; set; }

        [TempData] public string StatusMessage { get; set; }
        [TempData] public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required, EmailAddress]
            public string Email { get; set; }

            [Required]
            public string UserId { get; set; }

            // Only required after we display the question
            public string SecurityAnswer { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ResetLink = null;
            StatusMessage = null;
            ErrorMessage = null;

            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByIdAsync(Input.UserId);
            if (user == null || string.IsNullOrWhiteSpace(user.Email) ||
                !string.Equals(user.Email, Input.Email, StringComparison.OrdinalIgnoreCase))
            {
                // Do not reveal details
                ErrorMessage = "Invalid information.";
                return Page();
            }

            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (sec == null)
            {
                ErrorMessage = "Security setup is missing for this user. Contact an administrator.";
                return Page();
            }

            // Show the question if we haven't yet
            if (string.IsNullOrWhiteSpace(sec.SecurityQuestion))
            {
                ErrorMessage = "Security question is not set. Contact an administrator.";
                return Page();
            }

            SecurityQuestion = sec.SecurityQuestion;

            // If they haven't provided an answer yet, stop here (page will show question)
            if (string.IsNullOrWhiteSpace(Input.SecurityAnswer))
            {
                ModelState.AddModelError(string.Empty, "Please answer the security question.");
                return Page();
            }

            if (string.IsNullOrWhiteSpace(sec.SecurityAnswerHash))
            {
                ErrorMessage = "Security answer is not set. Contact an administrator.";
                return Page();
            }

            // Verify answer
            var answerOk = SecurityAnswerHasher.Verify(Input.SecurityAnswer, sec.SecurityAnswerHash);
            if (!answerOk)
            {
                ErrorMessage = "Invalid security answer.";
                return Page();
            }

            // Generate reset token + link
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);

            ResetLink = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "Identity", userId = user.Id, email = user.Email, code },
                protocol: Request.Scheme);

            // Log as "sent email" (Option A / enterprise audit trail)
            _db.SentEmails.Add(new SentEmail
            {
                ToEmail = user.Email,
                ToUserId = user.Id,
                Subject = "Password Reset Request",
                BodyHtml = $"A password reset was requested. Reset link:<br/><a href=\"{ResetLink}\">{ResetLink}</a>",
                SentAtUtc = DateTime.UtcNow,
                SentByUserId = null,
                Channel = "OutboxDb"
            });

            await _db.SaveChangesAsync();

            StatusMessage = "Reset link generated and logged. (Dev mode link shown below.)";
            return Page();
        }
    }
}