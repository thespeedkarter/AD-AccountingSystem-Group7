using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.ChartOfAccounts
{
    [Authorize] // all logged-in users can view
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<ChartAccount> Accounts { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; } // name or number

        [BindProperty(SupportsGet = true)]
        public AccountCategory? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Subcategory { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowInactive { get; set; } = false;

        public bool IsAdmin => User.IsInRole("Administrator");

        public async Task OnGetAsync()
        {
            IQueryable<ChartAccount> q = _db.ChartAccounts.AsNoTracking();

            if (!ShowInactive)
                q = q.Where(a => a.IsActive);

            if (Category.HasValue)
                q = q.Where(a => a.Category == Category.Value);

            if (!string.IsNullOrWhiteSpace(Subcategory))
                q = q.Where(a => a.Subcategory != null && a.Subcategory.Contains(Subcategory));

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();

                if (int.TryParse(s, out var acctNum))
                    q = q.Where(a => a.AccountNumber == acctNum);
                else
                    q = q.Where(a => a.AccountName.Contains(s));
            }

            Accounts = await q
                .OrderBy(a => a.AccountNumber)
                .ToListAsync();
        }
    }
}
