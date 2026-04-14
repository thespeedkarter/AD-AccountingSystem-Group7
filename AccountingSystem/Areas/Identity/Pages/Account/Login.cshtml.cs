#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AccountingSystem.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ApplicationDbContext db,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _db = db;
            _logger = logger;
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

            returnUrl ??= Url.Content("~/Dashboard");
            ReturnUrl = returnUrl;

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/Dashboard");
            ReturnUrl = returnUrl;

            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            var input = Input.UsernameOrEmail?.Trim();
            if (string.IsNullOrWhiteSpace(input))
            {
                ModelState.AddModelError(string.Empty, "Enter username or email.");
                return Page();
            }

            IdentityUser user = await _userManager.FindByNameAsync(input);
            if (user == null)
                user = await _userManager.FindByEmailAsync(input);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return Page();
            }

            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (sec == null)
            {
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

            var daysLeft = (sec.PasswordExpiresAt - now).TotalDays;
            if (daysLeft <= 3)
            {
                ModelState.AddModelError(string.Empty, $"Warning: your password expires in {Math.Ceiling(daysLeft)} day(s).");
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in: {UserId}", user.Id);
                return RedirectToPage("/Dashboard");
            }

            if (result.IsLockedOut)
            {
                sec.SuspendedFrom = DateTime.UtcNow;
                sec.SuspendedUntil = DateTime.UtcNow.AddMinutes(30);
                await _db.SaveChangesAsync();

                ModelState.AddModelError(string.Empty, "Account locked due to too many failed login attempts. Try again later.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }
    }
}