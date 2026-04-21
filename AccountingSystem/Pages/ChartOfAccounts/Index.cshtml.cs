using System.Text;
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
            Accounts = await BuildQuery()
                .OrderBy(a => a.OrderCode)
                .ThenBy(a => a.AccountNumber)
                .Take(500)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostExportCsvAsync(
            string? search,
            AccountCategory? category,
            string? subcategory,
            decimal? amount,
            bool showInactive)
        {
            Search = search;
            Category = category;
            Subcategory = subcategory;
            Amount = amount;
            ShowInactive = showInactive;

            var rows = await BuildQuery()
                .OrderBy(a => a.OrderCode)
                .ThenBy(a => a.AccountNumber)
                .ToListAsync();

            var sb = new StringBuilder();
            sb.AppendLine("ChartAccountId,AccountNumber,AccountName,Category,Subcategory,Statement,NormalSide,InitialBalance,Debit,Credit,Balance,Status,AddedAtUtc");

            foreach (var a in rows)
            {
                var accountName = EscapeCsv(a.AccountName);
                var sub = EscapeCsv(a.Subcategory ?? "");
                var statement = EscapeCsv(a.Statement);
                var status = a.IsActive ? "Active" : "Inactive";

                sb.AppendLine(
                    $"{a.ChartAccountId}," +
                    $"{a.AccountNumber}," +
                    $"\"{accountName}\"," +
                    $"{a.Category}," +
                    $"\"{sub}\"," +
                    $"\"{statement}\"," +
                    $"{a.NormalSide}," +
                    $"{a.InitialBalance:0.00}," +
                    $"{a.Debit:0.00}," +
                    $"{a.Credit:0.00}," +
                    $"{a.Balance:0.00}," +
                    $"{status}," +
                    $"{a.AddedAtUtc:yyyy-MM-dd HH:mm:ss}"
                );
            }

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "ChartOfAccounts.csv");
        }

        private IQueryable<ChartAccount> BuildQuery()
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

            return q;
        }

        private static string EscapeCsv(string value)
        {
            return value.Replace("\"", "\"\"");
        }
    }
}