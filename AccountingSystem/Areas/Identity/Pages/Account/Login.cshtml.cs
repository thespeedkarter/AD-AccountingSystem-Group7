#nullable disable

using System.ComponentModel.DataAnnotations;
using AccountingSystem.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext db)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string ErrorMessage { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Username or Email")]
            public string UsernameOrEmail { get; set; }

            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Display(Name = "Remember me?")]
            public bool RememberMe { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            returnUrl ??= Url.Content("~/");
            ReturnUrl = returnUrl;

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            // Support login by username OR email
            IdentityUser user = await _userManager.FindByNameAsync(Input.UsernameOrEmail);
            if (user == null)
                user = await _userManager.FindByEmailAsync(Input.UsernameOrEmail);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            // Sprint checks using UserSecurities
            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (sec != null)
            {
                if (!sec.IsActive)
                {
                    ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact an administrator.");
                    return Page();
                }

                var now = DateTime.UtcNow;

                if (sec.SuspendedFrom.HasValue && sec.SuspendedUntil.HasValue
                    && now >= sec.SuspendedFrom.Value && now <= sec.SuspendedUntil.Value)
                {
                    ModelState.AddModelError(string.Empty, $"Your account is suspended until {sec.SuspendedUntil.Value.ToLocalTime()}.");
                    return Page();
                }

                if (now > sec.PasswordExpiresAt)
                {
                    ModelState.AddModelError(string.Empty, "Your password has expired. Please use 'Forgot password' to reset it.");
                    return Page();
                }

                // 3-day warning (Sprint #15)
                var daysLeft = (sec.PasswordExpiresAt - now).TotalDays;
                if (daysLeft <= 3)
                {
                    ModelState.AddModelError(string.Empty, $"Warning: your password expires in {Math.Ceiling(daysLeft)} day(s).");
                }
            }

            // lockoutOnFailure: true to enforce 3 attempts (Sprint #13)
            var result = await _signInManager.PasswordSignInAsync(user.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
                return LocalRedirect(returnUrl);

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Account locked due to too many failed login attempts.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}
