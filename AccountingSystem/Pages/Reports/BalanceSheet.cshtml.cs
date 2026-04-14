using AccountingSystem.Models;
using AccountingSystem.Services;
using AccountingSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccountingSystem.Pages.Reports
{
    [Authorize(Roles = "Manager")]
    public class BalanceSheetModel : PageModel
    {
        private readonly IFinancialReportService _reports;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public BalanceSheetModel(IFinancialReportService reports, ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _reports = reports;
            _db = db;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)] public DateTime? AsOfDate { get; set; }
        [TempData] public string? StatusMessage { get; set; }

        public List<StatementRow> Rows { get; set; } = new();

        public async Task OnGetAsync()
        {
            Rows = await _reports.GetBalanceSheetAsync(AsOfDate);
        }
        public async Task<IActionResult> OnPostExportCsvAsync(DateTime? asOfDate)
        {
            var rows = await _reports.GetBalanceSheetAsync(asOfDate);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("AccountNumber,AccountName,Subcategory,Amount");

            foreach (var r in rows)
            {
                sb.AppendLine($"{r.AccountNumber},\"{r.AccountName}\",\"{r.Subcategory}\",{r.Amount:0.00}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "BalanceSheet.csv");
        }

        public async Task<IActionResult> OnPostEmailAsync(DateTime? asOfDate)
        {
            _db.SentEmails.Add(new SentEmail
            {
                ToEmail = User.Identity?.Name ?? "manager@local.test",
                Subject = "Balance Sheet Report",
                BodyHtml = "Balance Sheet generated.",
                SentByUserId = _userManager.GetUserId(User),
                SentAtUtc = DateTime.UtcNow,
                Channel = "OutboxDb"
            });

            await _db.SaveChangesAsync();
            StatusMessage = "Balance Sheet emailed to outbox.";
            return RedirectToPage(new { AsOfDate = asOfDate?.ToString("yyyy-MM-dd") });
        }
    }
}