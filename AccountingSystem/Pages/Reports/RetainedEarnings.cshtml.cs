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
    public class RetainedEarningsModel : PageModel
    {
        private readonly IFinancialReportService _reports;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public RetainedEarningsModel(IFinancialReportService reports, ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _reports = reports;
            _db = db;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)] public DateTime? From { get; set; }
        [BindProperty(SupportsGet = true)] public DateTime? To { get; set; }
        [TempData] public string? StatusMessage { get; set; }

        public List<StatementRow> Rows { get; set; } = new();

        public async Task OnGetAsync()
        {
            Rows = await _reports.GetRetainedEarningsAsync(From, To);
        }

        public async Task<IActionResult> OnPostExportCsvAsync(DateTime? from, DateTime? to)
        {
            var rows = await _reports.GetRetainedEarningsAsync(from, to);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("AccountNumber,AccountName,Subcategory,Amount");

            foreach (var r in rows)
            {
                sb.AppendLine($"{r.AccountNumber},\"{r.AccountName}\",\"{r.Subcategory}\",{r.Amount:0.00}");
            }

            sb.AppendLine($",,,{rows.Sum(x => x.Amount):0.00}");

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "RetainedEarnings.csv");
        }

        public async Task<IActionResult> OnPostEmailAsync(DateTime? from, DateTime? to)
        {
            _db.SentEmails.Add(new SentEmail
            {
                ToEmail = User.Identity?.Name ?? "manager@local.test",
                Subject = "Retained Earnings Report",
                BodyHtml = "Retained Earnings report generated.",
                SentByUserId = _userManager.GetUserId(User),
                SentAtUtc = DateTime.UtcNow,
                Channel = "OutboxDb"
            });

            await _db.SaveChangesAsync();
            StatusMessage = "Retained Earnings report emailed to outbox.";
            return RedirectToPage(new { From = from?.ToString("yyyy-MM-dd"), To = to?.ToString("yyyy-MM-dd") });
        }
    }
}