using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Ledger
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public ChartAccount Account { get; set; } = default!;
        public List<LedgerEntry> Entries { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int accountId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; } // amount

        public async Task<IActionResult> OnGetAsync()
        {
            var acct = await _db.ChartAccounts.AsNoTracking().FirstOrDefaultAsync(a => a.ChartAccountId == accountId);
            if (acct == null) return NotFound();

            Account = acct;

            IQueryable<LedgerEntry> q = _db.LedgerEntries
                .AsNoTracking()
                .Where(l => l.ChartAccountId == accountId)
                .OrderBy(l => l.EntryDate)
                .ThenBy(l => l.LedgerEntryId);

            if (From.HasValue) q = q.Where(l => l.EntryDate >= From.Value.Date);
            if (To.HasValue) q = q.Where(l => l.EntryDate <= To.Value.Date);

            if (!string.IsNullOrWhiteSpace(Search) && decimal.TryParse(Search.Trim(), out var amt))
                q = q.Where(l => l.Debit == amt || l.Credit == amt);

            Entries = await q.Take(500).ToListAsync();

            return Page();
        }
    }
}
