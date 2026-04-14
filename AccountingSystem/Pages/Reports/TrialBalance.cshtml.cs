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
    public class TrialBalanceModel : PageModel
    {
        private readonly IFinancialReportService _reports;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public TrialBalanceModel(
            IFinancialReportService reports,
            ApplicationDbContext db,
            UserManager<IdentityUser> userManager)
        {
            _reports = reports;
            _db = db;
            _userManager = userManager;
        }

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public List<TrialBalanceRow> Rows { get; set; } = new();
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }

        public async Task OnGetAsync()
        {
            Rows = await _reports.GetTrialBalanceAsync(From, To);
            TotalDebit = Rows.Sum(x => x.Debit);
            TotalCredit = Rows.Sum(x => x.Credit);
        }
        public async Task<IActionResult> OnPostExportCsvAsync(DateTime? from, DateTime? to)
        {
            var rows = await _reports.GetTrialBalanceAsync(from, to);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("AccountNumber,AccountName,Debit,Credit");

            foreach (var r in rows)
            {
                sb.AppendLine($"{r.AccountNumber},\"{r.AccountName}\",{r.Debit:0.00},{r.Credit:0.00}");
            }

            sb.AppendLine($",Totals,{rows.Sum(x => x.Debit):0.00},{rows.Sum(x => x.Credit):0.00}");

            var bytes = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", "TrialBalance.csv");
        }

        public async Task<IActionResult> OnPostEmailAsync(DateTime? from, DateTime? to)
        {
            var rows = await _reports.GetTrialBalanceAsync(from, to);

            _db.SentEmails.Add(new SentEmail
            {
                ToEmail = User.Identity?.Name ?? "manager@local.test",
                Subject = "Trial Balance Report",
                BodyHtml = $"Trial Balance generated with {rows.Count} row(s).",
                SentByUserId = _userManager.GetUserId(User),
                SentAtUtc = DateTime.UtcNow,
                Channel = "OutboxDb"
            });

            await _db.SaveChangesAsync();

            StatusMessage = "Trial Balance emailed to outbox.";
            return RedirectToPage(new { From = from?.ToString("yyyy-MM-dd"), To = to?.ToString("yyyy-MM-dd") });
        }
    }
}