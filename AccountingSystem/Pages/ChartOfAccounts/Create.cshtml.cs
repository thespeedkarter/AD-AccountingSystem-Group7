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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public ChartAccount Input { get; set; } = new();

        public void OnGet()
        {
            Input.IsActive = true;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // duplicate checks
            if (await _db.ChartAccounts.AnyAsync(a => a.AccountNumber == Input.AccountNumber))
                ModelState.AddModelError(string.Empty, "Duplicate account number is not allowed.");

            if (await _db.ChartAccounts.AnyAsync(a => a.AccountName == Input.AccountName))
                ModelState.AddModelError(string.Empty, "Duplicate account name is not allowed.");

            if (!ModelState.IsValid)
                return Page();

            Input.AddedAtUtc = DateTime.UtcNow;
            Input.AddedByUserId = _userManager.GetUserId(User);
            Input.IsActive = true;

            _db.ChartAccounts.Add(Input);
            await _db.SaveChangesAsync();

            _db.EventLogs.Add(new EventLog
            {
                TableName = "ChartAccounts",
                RecordId = Input.ChartAccountId,
                Action = (int)EventAction.Create,
                BeforeJson = null,
                AfterJson = JsonSerializer.Serialize(new
                {
                    Input.ChartAccountId,
                    Input.AccountNumber,
                    Input.AccountName,
                    Input.Category,
                    Input.Subcategory,
                    Input.InitialBalance,
                    Input.Balance,
                    Input.IsActive
                }),
                UserId = _userManager.GetUserId(User),
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            TempData["StatusMessage"] = "Account created.";
            return RedirectToPage("./Index");
        }
    }
}