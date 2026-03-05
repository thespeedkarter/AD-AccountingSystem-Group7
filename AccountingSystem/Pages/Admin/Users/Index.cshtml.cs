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
    public class IndexModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public IndexModel(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        [TempData] public string? StatusMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Role { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowInactive { get; set; }

        public List<string> AllRoles { get; set; } = new();

        public List<UserRow> Users { get; set; } = new();

        public class UserRow
        {
            public string UserId { get; set; } = "";
            public string UserName { get; set; } = "";
            public string Email { get; set; } = "";
            public string Role { get; set; } = "";
            public bool IsActive { get; set; } = true;
            public bool IsLockedOut { get; set; }
            public string PasswordExpiresAtLocal { get; set; } = "";
        }

        public async Task OnGetAsync()
        {
            // Roles dropdown
            AllRoles = new List<string> { "Administrator", "Manager", "Accountant" };

            // Base query: Identity users
            var q = _userManager.Users.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                q = q.Where(u =>
                    (u.UserName != null && u.UserName.Contains(s)) ||
                    (u.Email != null && u.Email.Contains(s)));
            }

            var identityUsers = await q
                .OrderBy(u => u.UserName)
                .Take(500)
                .ToListAsync();

            // Pull UserSecurity rows for the same IDs
            var ids = identityUsers.Select(u => u.Id).ToList();

            var securities = await _db.UserSecurities
                .AsNoTracking()
                .Where(x => ids.Contains(x.UserId))
                .ToDictionaryAsync(x => x.UserId);

            // Build rows
            var rows = new List<UserRow>();

            foreach (var u in identityUsers)
            {
                var roles = await _userManager.GetRolesAsync(u);
                var role = roles.FirstOrDefault() ?? "";

                // filter by role if requested
                if (!string.IsNullOrWhiteSpace(Role) && !string.Equals(role, Role, StringComparison.OrdinalIgnoreCase))
                    continue;

                securities.TryGetValue(u.Id, out var sec);

                var isActive = sec?.IsActive ?? true;
                if (!ShowInactive && !isActive)
                    continue;

                var expires = sec?.PasswordExpiresAt ?? DateTime.UtcNow.AddDays(90);

                var lockoutEnd = u.LockoutEnd;
                var locked = lockoutEnd.HasValue && lockoutEnd.Value.UtcDateTime > DateTime.UtcNow;

                rows.Add(new UserRow
                {
                    UserId = u.Id,
                    UserName = u.UserName ?? "",
                    Email = u.Email ?? "",
                    Role = string.IsNullOrWhiteSpace(role) ? "(none)" : role,
                    IsActive = isActive,
                    IsLockedOut = locked,
                    PasswordExpiresAtLocal = expires.ToLocalTime().ToString("yyyy-MM-dd hh:mm tt")
                });
            }

            Users = rows;
        }
    }
}