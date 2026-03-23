using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.ChartOfAccounts
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<ChartAccount> Accounts { get; set; } = new();

        [BindProperty(SupportsGet = true)] public string? Search { get; set; }
        [BindProperty(SupportsGet = true)] public AccountCategory? Category { get; set; }
        [BindProperty(SupportsGet = true)] public string? Subcategory { get; set; }
        [BindProperty(SupportsGet = true)] public decimal? Amount { get; set; }
        [BindProperty(SupportsGet = true)] public bool ShowInactive { get; set; }

        [TempData] public string? StatusMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public bool IsAdmin => User.IsInRole("Administrator");

        public async Task OnGetAsync()
        {
            IQueryable<ChartAccount> q = _db.ChartAccounts.AsNoTracking();

            if (!ShowInactive)
                q = q.Where(a => a.IsActive);

            if (Category.HasValue)
                q = q.Where(a => a.Category == Category.Value);

            if (!string.IsNullOrWhiteSpace(Subcategory))
            {
                var sub = Subcategory.Trim();
                q = q.Where(a => a.Subcategory != null && a.Subcategory.Contains(sub));
            }

            if (Amount.HasValue)
                q = q.Where(a => a.Balance == Amount.Value);

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();

                if (int.TryParse(s, out var acctNumber))
                {
                    q = q.Where(a => a.AccountNumber == acctNumber || a.AccountName.Contains(s));
                }
                else
                {
                    q = q.Where(a => a.AccountName.Contains(s));
                }
            }

            Accounts = await q
                .OrderBy(a => a.OrderCode)
                .ThenBy(a => a.AccountNumber)
                .Take(500)
                .ToListAsync();
        }
    }
}