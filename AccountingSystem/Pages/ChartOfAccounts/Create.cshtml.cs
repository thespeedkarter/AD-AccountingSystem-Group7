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
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IEventLogger _logger;

        public CreateModel(ApplicationDbContext db, UserManager<IdentityUser> userManager, IEventLogger logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public ChartAccount Account { get; set; } = new ChartAccount();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            // Duplicates not allowed
            if (await _db.ChartAccounts.AnyAsync(a => a.AccountName == Account.AccountName))
            {
                ModelState.AddModelError(string.Empty, "Account name already exists.");
                return Page();
            }

            if (await _db.ChartAccounts.AnyAsync(a => a.AccountNumber == Account.AccountNumber))
            {
                ModelState.AddModelError(string.Empty, "Account number already exists.");
                return Page();
            }

            Account.AddedAtUtc = DateTime.UtcNow;
            Account.AddedByUserId = _userManager.GetUserId(User);
            Account.Balance = Account.InitialBalance;

            _db.ChartAccounts.Add(Account);
            await _db.SaveChangesAsync();

            // Event log: Created
            await _logger.LogAsync(
                tableName: "ChartAccounts",
                recordId: Account.ChartAccountId,
                action: EventAction.Created,
                before: null,
                after: new
                {
                    Account.ChartAccountId,
                    Account.AccountName,
                    Account.AccountNumber,
                    Account.Description,
                    Account.NormalSide,
                    Account.Category,
                    Account.Subcategory,
                    Account.InitialBalance,
                    Account.Debit,
                    Account.Credit,
                    Account.Balance,
                    Account.OrderCode,
                    Account.Statement,
                    Account.Comment,
                    Account.IsActive
                },
                userId: Account.AddedByUserId
            );

            return RedirectToPage("./Index");
        }
    }
}
