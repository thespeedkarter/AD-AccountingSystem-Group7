using AccountingSystem.Data;
using AccountingSystem.Models;
using AccountingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Administrator")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEventLogger _logger;

        public EditModel(ApplicationDbContext db, UserManager<IdentityUser> userManager, IEventLogger logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public ChartAccount Account { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var acct = await _db.ChartAccounts.FirstOrDefaultAsync(a => a.ChartAccountId == id);
            if (acct == null) return NotFound();
            Account = acct;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Load existing for before-image
            var existing = await _db.ChartAccounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.ChartAccountId == Account.ChartAccountId);

            if (existing == null) return NotFound();

            // Prevent duplicates (exclude self)
            if (await _db.ChartAccounts.AnyAsync(a => a.ChartAccountId != Account.ChartAccountId && a.AccountName == Account.AccountName))
            {
                ModelState.AddModelError(string.Empty, "Account name already exists.");
                return Page();
            }

            if (await _db.ChartAccounts.AnyAsync(a => a.ChartAccountId != Account.ChartAccountId && a.AccountNumber == Account.AccountNumber))
            {
                ModelState.AddModelError(string.Empty, "Account number already exists.");
                return Page();
            }

            _db.Attach(Account).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            var userId = _userManager.GetUserId(User);

            await _logger.LogAsync(
                tableName: "ChartAccounts",
                recordId: Account.ChartAccountId,
                action: EventAction.Updated,
                before: existing,
                after: Account,
                userId: userId
            );

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostDeactivateAsync(int id)
        {
            var acct = await _db.ChartAccounts.FirstOrDefaultAsync(a => a.ChartAccountId == id);
            if (acct == null) return NotFound();

            if (acct.Balance > 0)
            {
                TempData["Error"] = "Accounts with a balance greater than zero cannot be deactivated.";
                return RedirectToPage("./Edit", new { id });
            }

            var before = new { acct.ChartAccountId, acct.AccountName, acct.AccountNumber, acct.IsActive, acct.Balance };

            acct.IsActive = false;
            await _db.SaveChangesAsync();

            await _logger.LogAsync(
                tableName: "ChartAccounts",
                recordId: acct.ChartAccountId,
                action: EventAction.Deactivated,
                before: before,
                after: new { acct.ChartAccountId, acct.AccountName, acct.AccountNumber, acct.IsActive, acct.Balance },
                userId: _userManager.GetUserId(User)
            );

            return RedirectToPage("./Index");
        }

        public async Task<IActionResult> OnPostActivateAsync(int id)
        {
            var acct = await _db.ChartAccounts.FirstOrDefaultAsync(a => a.ChartAccountId == id);
            if (acct == null) return NotFound();

            var before = new { acct.ChartAccountId, acct.AccountName, acct.AccountNumber, acct.IsActive, acct.Balance };

            acct.IsActive = true;
            await _db.SaveChangesAsync();

            await _logger.LogAsync(
                tableName: "ChartAccounts",
                recordId: acct.ChartAccountId,
                action: EventAction.Activated,
                before: before,
                after: new { acct.ChartAccountId, acct.AccountName, acct.AccountNumber, acct.IsActive, acct.Balance },
                userId: _userManager.GetUserId(User)
            );

            return RedirectToPage("./Edit", new { id });
        }
    }
}
