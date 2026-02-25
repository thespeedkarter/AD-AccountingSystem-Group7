using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Journal
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<JournalEntry> Entries { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public JournalStatus? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public bool IsManager => User.IsInRole("Manager");

        public async Task OnGetAsync()
        {
            IQueryable<JournalEntry> q = _db.JournalEntries
                .AsNoTracking()
                .Include(e => e.Lines)
                    .ThenInclude(l => l.ChartAccount);

            if (Status.HasValue)
                q = q.Where(e => e.Status == Status.Value);

            if (From.HasValue)
                q = q.Where(e => e.EntryDate >= From.Value.Date);

            if (To.HasValue)
                q = q.Where(e => e.EntryDate <= To.Value.Date);

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();

                if (DateTime.TryParse(s, out var dt))
                {
                    var d = dt.Date;
                    q = q.Where(e => e.EntryDate == d);
                }
                else if (decimal.TryParse(s, out var amt))
                {
                    q = q.Where(e => e.Lines.Any(l => l.Debit == amt || l.Credit == amt));
                }
                else
                {
                    q = q.Where(e => e.Lines.Any(l => l.ChartAccount != null && l.ChartAccount.AccountName.Contains(s)));
                }
            }

            Entries = await q
                .OrderByDescending(e => e.EntryDate)
                .ThenByDescending(e => e.JournalEntryId)
                .Take(200)
                .ToListAsync();
        }

        // OPTION 1: Post only after approval
        public async Task<IActionResult> OnPostPostEntryAsync(int id)
        {
            if (!User.IsInRole("Manager"))
                return Forbid();

            var je = await _db.JournalEntries
                .Include(e => e.Lines)
                    .ThenInclude(l => l.ChartAccount)
                .FirstOrDefaultAsync(e => e.JournalEntryId == id);

            if (je == null)
                return NotFound();

            if (je.Status != JournalStatus.Approved)
            {
                StatusMessage = "Only Approved entries can be posted.";
                return RedirectToPage();
            }

            if (je.PostedAtUtc != null)
            {
                StatusMessage = "This entry has already been posted.";
                return RedirectToPage();
            }

            // Safety: must balance
            var totalDebit = je.Lines.Sum(l => l.Debit);
            var totalCredit = je.Lines.Sum(l => l.Credit);
            if (totalDebit != totalCredit)
            {
                StatusMessage = "Cannot post: debits do not equal credits.";
                return RedirectToPage();
            }

            var nowUtc = DateTime.UtcNow;
            var postedByUserId = _userManager.GetUserId(User);

            // Mark as posted on the journal entry
            je.PostedAtUtc = nowUtc;
            je.PostedByUserId = postedByUserId;
            je.Status = JournalStatus.Posted;

            // Create ledger rows
            // Option B: ledger drives account balances
            foreach (var line in je.Lines)
            {
                // compute balance-after: last ledger balance for that account + (debit-credit)
                var lastBalance = await _db.LedgerEntries
                    .Where(x => x.ChartAccountId == line.ChartAccountId)
                    .OrderByDescending(x => x.EntryDate)
                    .ThenByDescending(x => x.LedgerEntryId)
                    .Select(x => (decimal?)x.BalanceAfter)
                    .FirstOrDefaultAsync();

                var startBalance = lastBalance ?? line.ChartAccount!.InitialBalance;
                var balanceAfter = startBalance + (line.Debit - line.Credit);

                _db.LedgerEntries.Add(new LedgerEntry
                {
                    ChartAccountId = line.ChartAccountId,
                    EntryDate = je.EntryDate,
                    JournalEntryId = je.JournalEntryId,
                    Description = je.Description,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    BalanceAfter = balanceAfter,
                    PostedAtUtc = nowUtc
                });
            }

            // Update ChartAccounts totals + balance using ledger
            // (fast method: recompute for only accounts touched)
            var touchedAccountIds = je.Lines.Select(l => l.ChartAccountId).Distinct().ToList();
            foreach (var acctId in touchedAccountIds)
            {
                var totals = await _db.LedgerEntries
                    .Where(x => x.ChartAccountId == acctId)
                    .GroupBy(x => x.ChartAccountId)
                    .Select(g => new
                    {
                        TotalDebit = g.Sum(x => x.Debit),
                        TotalCredit = g.Sum(x => x.Credit),
                        EndBalance = g.OrderByDescending(x => x.EntryDate).ThenByDescending(x => x.LedgerEntryId).Select(x => x.BalanceAfter).FirstOrDefault()
                    })
                    .FirstOrDefaultAsync();

                var acct = await _db.ChartAccounts.FirstOrDefaultAsync(a => a.ChartAccountId == acctId);
                if (acct != null && totals != null)
                {
                    acct.Debit = totals.TotalDebit;
                    acct.Credit = totals.TotalCredit;
                    acct.Balance = totals.EndBalance;
                }
            }

            // Event log (only if it matches columns you used earlier)
            if (await _db.Database.ExecuteSqlRawAsync(@"
                IF OBJECT_ID('dbo.EventLogs','U') IS NOT NULL
                BEGIN
                    IF COL_LENGTH('dbo.EventLogs','EntityName') IS NOT NULL
                       AND COL_LENGTH('dbo.EventLogs','EntityId') IS NOT NULL
                       AND COL_LENGTH('dbo.EventLogs','OccurredAtUtc') IS NOT NULL
                       AND COL_LENGTH('dbo.EventLogs','UserId') IS NOT NULL
                    BEGIN
                        INSERT dbo.EventLogs (EntityName, EntityId, OccurredAtUtc, UserId)
                        VALUES ('JournalEntry', CAST({0} as nvarchar(50)), {1}, {2});
                    END
                END
            ", je.JournalEntryId, nowUtc, postedByUserId) >= 0)
            {
                // no-op
            }

            await _db.SaveChangesAsync();

            StatusMessage = $"Posted Journal Entry #{je.JournalEntryId}. Ledger and balances updated.";
            return RedirectToPage(new { Status = (int)JournalStatus.Approved });
        }
    }
}