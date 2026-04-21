using System.Text;
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
        public bool? IsAdjusting { get; set; }

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
            Entries = await BuildQuery()
                .OrderByDescending(e => e.EntryDate)
                .ThenByDescending(e => e.JournalEntryId)
                .Take(200)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostExportCsvAsync(
            JournalStatus? status,
            bool? isAdjusting,
            DateTime? from,
            DateTime? to,
            string? search)
        {
            Status = status;
            IsAdjusting = isAdjusting;
            From = from;
            To = to;
            Search = search;

            var rows = await BuildQuery()
                .OrderByDescending(e => e.EntryDate)
                .ThenByDescending(e => e.JournalEntryId)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("JournalEntryId,EntryDate,Description,Status,Type,TotalDebit,TotalCredit,Posted,CreatedAtUtc");

            foreach (var e in rows)
            {
                var totalDebit = e.Lines.Sum(l => l.Debit);
                var totalCredit = e.Lines.Sum(l => l.Credit);
                var description = EscapeCsv(e.Description ?? "");
                var type = e.IsAdjusting ? "Adjusting" : "Regular";
                var posted = e.PostedAtUtc.HasValue ? "Yes" : "No";

                sb.AppendLine(
                    $"{e.JournalEntryId}," +
                    $"{e.EntryDate:yyyy-MM-dd}," +
                    $"\"{description}\"," +
                    $"{e.Status}," +
                    $"{type}," +
                    $"{totalDebit:0.00}," +
                    $"{totalCredit:0.00}," +
                    $"{posted}," +
                    $"{e.CreatedAtUtc:yyyy-MM-dd HH:mm:ss}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "JournalEntries.csv");
        }

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

            var totalDebit = je.Lines.Sum(l => l.Debit);
            var totalCredit = je.Lines.Sum(l => l.Credit);
            if (totalDebit != totalCredit)
            {
                StatusMessage = "Cannot post: debits do not equal credits.";
                return RedirectToPage();
            }

            var nowUtc = DateTime.UtcNow;
            var postedByUserId = _userManager.GetUserId(User);

            je.PostedAtUtc = nowUtc;
            je.PostedByUserId = postedByUserId;
            je.Status = JournalStatus.Posted;

            foreach (var line in je.Lines)
            {
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
                        EndBalance = g.OrderByDescending(x => x.EntryDate)
                                      .ThenByDescending(x => x.LedgerEntryId)
                                      .Select(x => x.BalanceAfter)
                                      .FirstOrDefault()
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

            _db.EventLogs.Add(new EventLog
            {
                TableName = "JournalEntries",
                RecordId = je.JournalEntryId,
                Action = (int)EventAction.Post,
                BeforeJson = null,
                AfterJson = System.Text.Json.JsonSerializer.Serialize(new
                {
                    je.JournalEntryId,
                    je.Status,
                    je.IsAdjusting,
                    je.PostedAtUtc,
                    je.PostedByUserId
                }),
                UserId = postedByUserId,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            StatusMessage = $"Posted Journal Entry #{je.JournalEntryId}. Ledger and balances updated.";
            return RedirectToPage(new { Status = (int)JournalStatus.Approved });
        }

        private IQueryable<JournalEntry> BuildQuery()
        {
            IQueryable<JournalEntry> q = _db.JournalEntries
                .AsNoTracking()
                .Include(e => e.Lines)
                    .ThenInclude(l => l.ChartAccount);

            if (Status.HasValue)
                q = q.Where(e => e.Status == Status.Value);

            if (IsAdjusting.HasValue)
                q = q.Where(e => e.IsAdjusting == IsAdjusting.Value);

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
                    q = q.Where(e =>
                        (e.Description != null && e.Description.Contains(s)) ||
                        e.Lines.Any(l => l.ChartAccount != null && l.ChartAccount.AccountName.Contains(s)));
                }
            }

            return q;
        }

        private static string EscapeCsv(string value)
        {
            return value.Replace("\"", "\"\"");
        }
    }
}