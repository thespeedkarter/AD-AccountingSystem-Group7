using System.Text.Json;
using AccountingSystem.Data;
using AccountingSystem.Models;
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

        public EditModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public ChartAccount Input { get; set; } = new();

        [TempData] public string? StatusMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var acct = await _db.ChartAccounts.FirstOrDefaultAsync(a => a.ChartAccountId == id);
            if (acct == null) return NotFound();

            Input = acct;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var existing = await _db.ChartAccounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.ChartAccountId == Input.ChartAccountId);

            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
                return Page();

            if (await _db.ChartAccounts.AnyAsync(a => a.ChartAccountId != Input.ChartAccountId && a.AccountNumber == Input.AccountNumber))
                ModelState.AddModelError(string.Empty, "Duplicate account number is not allowed.");

            if (await _db.ChartAccounts.AnyAsync(a => a.ChartAccountId != Input.ChartAccountId && a.AccountName == Input.AccountName))
                ModelState.AddModelError(string.Empty, "Duplicate account name is not allowed.");

            if (!ModelState.IsValid)
                return Page();

            _db.ChartAccounts.Update(Input);
            await _db.SaveChangesAsync();

            _db.EventLogs.Add(new EventLog
            {
                TableName = "ChartAccounts",
                RecordId = Input.ChartAccountId,
                Action = (int)EventAction.Update,
                BeforeJson = JsonSerializer.Serialize(existing),
                AfterJson = JsonSerializer.Serialize(Input),
                UserId = _userManager.GetUserId(User),
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            StatusMessage = "Account updated.";
            return RedirectToPage("./Edit", new { id = Input.ChartAccountId });
        }

        public async Task<IActionResult> OnPostDeactivateAsync()
        {
            var acct = await _db.ChartAccounts.FirstOrDefaultAsync(a => a.ChartAccountId == Input.ChartAccountId);
            if (acct == null) return NotFound();

            if (acct.Balance > 0)
            {
                ErrorMessage = "Accounts with balance greater than zero cannot be deactivated.";
                return RedirectToPage("./Edit", new { id = acct.ChartAccountId });
            }

            var before = JsonSerializer.Serialize(acct);

            acct.IsActive = false;
            await _db.SaveChangesAsync();

            _db.EventLogs.Add(new EventLog
            {
                TableName = "ChartAccounts",
                RecordId = acct.ChartAccountId,
                Action = (int)EventAction.Deactivate,
                BeforeJson = before,
                AfterJson = JsonSerializer.Serialize(acct),
                UserId = _userManager.GetUserId(User),
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            StatusMessage = "Account deactivated.";
            return RedirectToPage("./Edit", new { id = acct.ChartAccountId });
        }
    }
}