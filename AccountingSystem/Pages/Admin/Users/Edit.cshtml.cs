using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Admin.Users
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditModel(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [TempData] public string? StatusMessage { get; set; }

        // Display-only
        public string? UserEmail { get; set; }
        public string RoleDisplay { get; set; } = "—";

        // Form fields
        [BindProperty(SupportsGet = true)]
        public string UserId { get; set; } = "";

        [BindProperty]
        public bool IsActive { get; set; } = true;

        // datetime-local expects "yyyy-MM-ddTHH:mm"
        [BindProperty]
        public string? SuspendedFromLocal { get; set; }

        [BindProperty]
        public string? SuspendedUntilLocal { get; set; }

        [BindProperty]
        public string SelectedRole { get; set; } = "Accountant";

        public List<string> AllRoles { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string userId)
        {
            UserId = userId;

            AllRoles = await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(r => r)
                .ToListAsync();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            UserEmail = user.Email;

            var roles = await _userManager.GetRolesAsync(user);
            RoleDisplay = roles.Count == 0 ? "—" : string.Join(", ", roles.OrderBy(r => r));

            // If user has roles, default dropdown to first
            if (roles.Count > 0)
                SelectedRole = roles.OrderBy(r => r).First();

            // Load security row (create if missing)
            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == userId);
            if (sec == null)
            {
                // Safe defaults
                sec = new UserSecurity
                {
                    UserId = userId,
                    IsActive = true,
                    PasswordExpiresAt = DateTime.UtcNow.AddDays(60),
                    PasswordLastChangedAt = DateTime.UtcNow
                };
                _db.UserSecurities.Add(sec);
                await _db.SaveChangesAsync();
            }

            IsActive = sec.IsActive;

            SuspendedFromLocal = sec.SuspendedFrom.HasValue
                ? sec.SuspendedFrom.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm")
                : null;

            SuspendedUntilLocal = sec.SuspendedUntil.HasValue
                ? sec.SuspendedUntil.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm")
                : null;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            AllRoles = await _roleManager.Roles
                .Select(r => r.Name!)
                .OrderBy(r => r)
                .ToListAsync();

            var user = await _userManager.FindByIdAsync(UserId);
            if (user == null) return NotFound();

            // Update roles (simple: keep only SelectedRole)
            if (string.IsNullOrWhiteSpace(SelectedRole) || !AllRoles.Contains(SelectedRole))
            {
                ModelState.AddModelError(string.Empty, "Please select a valid role.");
                return await OnGetAsync(UserId);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            // Remove roles not selected
            var remove = currentRoles.Where(r => r != SelectedRole).ToList();
            if (remove.Count > 0)
                await _userManager.RemoveFromRolesAsync(user, remove);

            // Add selected role if missing
            if (!currentRoles.Contains(SelectedRole))
                await _userManager.AddToRoleAsync(user, SelectedRole);

            // Update security row
            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == UserId);
            if (sec == null)
            {
                sec = new UserSecurity
                {
                    UserId = UserId,
                    PasswordExpiresAt = DateTime.UtcNow.AddDays(60),
                    PasswordLastChangedAt = DateTime.UtcNow
                };
                _db.UserSecurities.Add(sec);
            }

            sec.IsActive = IsActive;

            // Parse datetime-local strings to UTC
            sec.SuspendedFrom = ParseLocalToUtcOrNull(SuspendedFromLocal);
            sec.SuspendedUntil = ParseLocalToUtcOrNull(SuspendedUntilLocal);

            // If only one is filled, treat as invalid
            if ((sec.SuspendedFrom.HasValue && !sec.SuspendedUntil.HasValue) ||
                (!sec.SuspendedFrom.HasValue && sec.SuspendedUntil.HasValue))
            {
                ModelState.AddModelError(string.Empty, "Provide BOTH Suspended From and Suspended Until, or leave both empty.");
                return await OnGetAsync(UserId);
            }

            if (sec.SuspendedFrom.HasValue && sec.SuspendedUntil.HasValue && sec.SuspendedFrom > sec.SuspendedUntil)
            {
                ModelState.AddModelError(string.Empty, "Suspended From must be before Suspended Until.");
                return await OnGetAsync(UserId);
            }

            await _db.SaveChangesAsync();

            StatusMessage = "User updated.";
            return RedirectToPage("./Edit", new { userId = UserId });
        }

        private static DateTime? ParseLocalToUtcOrNull(string? local)
        {
            if (string.IsNullOrWhiteSpace(local)) return null;

            // datetime-local comes in as local time without timezone
            if (!DateTime.TryParse(local, out var dtLocal)) return null;

            // Treat parsed DateTime as local and convert to UTC
            return DateTime.SpecifyKind(dtLocal, DateTimeKind.Local).ToUniversalTime();
        }
    }
}