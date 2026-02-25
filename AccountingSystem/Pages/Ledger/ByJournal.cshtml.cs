using AccountingSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Ledger
{
    [Authorize(Roles = "Administrator,Manager,Accountant")]
    public class ByJournalModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public ByJournalModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public int JournalEntryId { get; set; }

        public string? ErrorMessage { get; set; }

        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }

        public string PostedAtLocal { get; set; } = "";

        public List<RowVm> Rows { get; set; } = new();

        public class RowVm
        {
            public int AccountNumber { get; set; }
            public string AccountName { get; set; } = "";
            public string? Description { get; set; }
            public decimal Debit { get; set; }
            public decimal Credit { get; set; }
            public decimal BalanceAfter { get; set; }
            public string PostedAtLocal { get; set; } = "";
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            JournalEntryId = id;

            // Load ledger rows for this journal entry
            var ledgerRows = await _db.LedgerEntries
                .AsNoTracking()
                .Include(l => l.ChartAccount)
                .Where(l => l.JournalEntryId == id)
                .OrderBy(l => l.LedgerEntryId)
                .ToListAsync();

            if (ledgerRows.Count == 0)
            {
                // Helpful message, not a hard error
                return Page();
            }

            TotalDebit = ledgerRows.Sum(x => x.Debit);
            TotalCredit = ledgerRows.Sum(x => x.Credit);

            // Display a single "posted at" (they should all share same tx time)
            var postedAtUtc = ledgerRows.Max(x => x.PostedAtUtc);
            PostedAtLocal = postedAtUtc.ToLocalTime().ToString("g");

            Rows = ledgerRows.Select(x => new RowVm
            {
                AccountNumber = x.ChartAccount?.AccountNumber ?? 0,
                AccountName = x.ChartAccount?.AccountName ?? "(Account missing)",
                Description = x.Description,
                Debit = x.Debit,
                Credit = x.Credit,
                BalanceAfter = x.BalanceAfter,
                PostedAtLocal = x.PostedAtUtc.ToLocalTime().ToString("g")
            }).ToList();

            return Page();
        }
    }
}