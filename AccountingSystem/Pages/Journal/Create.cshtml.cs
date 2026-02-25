using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Journal
{
    [Authorize(Roles = "Manager,Accountant")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<ChartAccount> AccountOptions { get; set; } = new();

        [BindProperty]
        public JournalEntry Entry { get; set; } = new JournalEntry
        {
            EntryDate = DateTime.Today
        };

        [BindProperty]
        public List<JournalLineInput> Lines { get; set; } = new();

        public class JournalLineInput
        {
            public int ChartAccountId { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public string? Memo { get; set; }
        }

        public async Task OnGetAsync()
        {
            AccountOptions = await _db.ChartAccounts
                .AsNoTracking()
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();

            // Start with 2 blank lines
            if (Lines.Count == 0)
            {
                Lines.Add(new JournalLineInput());
                Lines.Add(new JournalLineInput());
            }
        }

        public async Task<IActionResult> OnPostAddLineAsync()
        {
            await LoadAccountsAsync();
            Lines.Add(new JournalLineInput());
            return Page();
        }

        public async Task<IActionResult> OnPostRemoveLineAsync(int index)
        {
            await LoadAccountsAsync();

            if (index >= 0 && index < Lines.Count)
                Lines.RemoveAt(index);

            if (Lines.Count == 0)
                Lines.Add(new JournalLineInput());

            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            await LoadAccountsAsync();

            // Basic validation: at least one debit and one credit line with > 0
            var hasDebit = Lines.Any(l => l.Debit > 0);
            var hasCredit = Lines.Any(l => l.Credit > 0);

            if (!hasDebit || !hasCredit)
            {
                ModelState.AddModelError(string.Empty, "Each transaction must have at least one debit and one credit.");
                return Page();
            }

            // No line can have both debit and credit > 0
            foreach (var l in Lines)
            {
                if (l.Debit > 0 && l.Credit > 0)
                {
                    ModelState.AddModelError(string.Empty, "A line cannot have both a debit and a credit amount.");
                    return Page();
                }
            }

            // Totals must balance
            var totalDebit = Lines.Sum(l => l.Debit);
            var totalCredit = Lines.Sum(l => l.Credit);

            if (totalDebit != totalCredit)
            {
                ModelState.AddModelError(string.Empty, $"Debits and credits must balance. Debits={totalDebit:N2}, Credits={totalCredit:N2}");
                return Page();
            }

            // Validate account IDs exist and are active
            var accountIds = Lines.Where(l => l.Debit > 0 || l.Credit > 0).Select(l => l.ChartAccountId).Distinct().ToList();
            var validCount = await _db.ChartAccounts.CountAsync(a => a.IsActive && accountIds.Contains(a.ChartAccountId));
            if (validCount != accountIds.Count)
            {
                ModelState.AddModelError(string.Empty, "One or more selected accounts are invalid or inactive.");
                return Page();
            }

            // Save JournalEntry + Lines
            Entry.Status = JournalStatus.Pending;
            Entry.CreatedAtUtc = DateTime.UtcNow;
            Entry.CreatedByUserId = _userManager.GetUserId(User);

            // Only keep non-zero lines
            var nonZeroLines = Lines
                .Where(l => l.Debit > 0 || l.Credit > 0)
                .ToList();

            Entry.Lines = nonZeroLines.Select(l => new JournalLine
            {
                ChartAccountId = l.ChartAccountId,
                Debit = l.Debit,
                Credit = l.Credit,
                Memo = l.Memo
            }).ToList();

            _db.JournalEntries.Add(Entry);
            await _db.SaveChangesAsync();

            return RedirectToPage("./Details", new { id = Entry.JournalEntryId });
        }

        private async Task LoadAccountsAsync()
        {
            AccountOptions = await _db.ChartAccounts
                .AsNoTracking()
                .Where(a => a.IsActive)
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
        }
    }
}
