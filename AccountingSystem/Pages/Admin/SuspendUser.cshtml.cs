using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class SuspendUserModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public SuspendUserModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<IdentityUser> Users { get; set; } = new();

        [BindProperty] public string? SelectedUserId { get; set; }
        [BindProperty] public DateTime? SuspendedFrom { get; set; }
        [BindProperty] public DateTime? SuspendedUntil { get; set; }
        [BindProperty] public bool DeactivateAccount { get; set; }

        public async Task OnGetAsync()
        {
            Users = await _db.Users.OrderBy(u => u.UserName).ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Users = await _db.Users.OrderBy(u => u.UserName).ToListAsync();

            if (string.IsNullOrWhiteSpace(SelectedUserId))
            {
                ModelState.AddModelError(string.Empty, "Please select a user.");
                return Page();
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == SelectedUserId);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "User not found.");
                return Page();
            }

            var sec = await _db.UserSecurities.FirstOrDefaultAsync(s => s.UserId == user.Id);
            if (sec == null)
            {
                sec = new UserSecurity { UserId = user.Id, IsActive = true };
                _db.UserSecurities.Add(sec);
            }

            if (DeactivateAccount)
            {
                sec.IsActive = false;
                sec.SuspendedFrom = null;
                sec.SuspendedUntil = null;
            }
            else
            {
                sec.IsActive = true;
                sec.SuspendedFrom = SuspendedFrom?.ToUniversalTime();
                sec.SuspendedUntil = SuspendedUntil?.ToUniversalTime();
            }

            await _db.SaveChangesAsync();

            TempData["Message"] = "User security settings updated.";
            return RedirectToPage();
        }
    }
}