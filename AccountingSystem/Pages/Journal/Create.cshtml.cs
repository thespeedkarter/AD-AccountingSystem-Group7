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
            await LoadAccountsAsync();

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

        public async Task<IActionResult> OnPostResetAsync()
        {
            await LoadAccountsAsync();

            Entry = new JournalEntry
            {
                EntryDate = DateTime.Today
            };

            Lines = new List<JournalLineInput>
            {
                new JournalLineInput(),
                new JournalLineInput()
            };

            ModelState.Clear();
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            await LoadAccountsAsync();

            var hasDebit = Lines.Any(l => l.Debit > 0);
            var hasCredit = Lines.Any(l => l.Credit > 0);

            if (!hasDebit || !hasCredit)
            {
                ModelState.AddModelError(string.Empty, "Each transaction must have at least one debit and one credit.");
                return Page();
            }

            foreach (var l in Lines)
            {
                if (l.Debit > 0 && l.Credit > 0)
                {
                    ModelState.AddModelError(string.Empty, "A line cannot have both a debit and a credit amount.");
                    return Page();
                }
            }

            var totalDebit = Lines.Sum(l => l.Debit);
            var totalCredit = Lines.Sum(l => l.Credit);

            if (totalDebit != totalCredit)
            {
                ModelState.AddModelError(string.Empty, $"Debits and credits must balance. Debits={totalDebit:N2}, Credits={totalCredit:N2}");
                return Page();
            }

            var nonZeroLines = Lines
                .Where(l => l.Debit > 0 || l.Credit > 0)
                .ToList();

            var accountIds = nonZeroLines.Select(l => l.ChartAccountId).Distinct().ToList();
            var validCount = await _db.ChartAccounts.CountAsync(a => a.IsActive && accountIds.Contains(a.ChartAccountId));
            if (validCount != accountIds.Count)
            {
                ModelState.AddModelError(string.Empty, "One or more selected accounts are invalid or inactive.");
                return Page();
            }

            Entry.Status = JournalStatus.Pending;
            Entry.CreatedAtUtc = DateTime.UtcNow;
            Entry.CreatedByUserId = _userManager.GetUserId(User);

            Entry.Lines = nonZeroLines.Select(l => new JournalLine
            {
                ChartAccountId = l.ChartAccountId,
                Debit = l.Debit,
                Credit = l.Credit,
                Memo = l.Memo
            }).ToList();

            _db.JournalEntries.Add(Entry);
            await _db.SaveChangesAsync();

            // Manager notification
            var managers = await (
                from u in _db.Users
                join ur in _db.UserRoles on u.Id equals ur.UserId
                join r in _db.Roles on ur.RoleId equals r.Id
                where r.Name == "Manager" && u.Email != null
                select new { u.Id, u.Email }
            ).ToListAsync();

            foreach (var m in managers)
            {
                _db.SentEmails.Add(new SentEmail
                {
                    ToEmail = m.Email!,
                    ToUserId = m.Id,
                    Subject = $"Journal Entry #{Entry.JournalEntryId} Submitted for Approval",
                    BodyHtml =
                        $@"<p>A journal entry has been submitted for approval.</p>
                           <p><strong>Journal Entry ID:</strong> {Entry.JournalEntryId}<br/>
                           <strong>Date:</strong> {Entry.EntryDate:d}<br/>
                           <strong>Description:</strong> {Entry.Description}</p>",
                    SentByUserId = _userManager.GetUserId(User),
                    SentAtUtc = DateTime.UtcNow,
                    Channel = "OutboxDb"
                });
            }

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