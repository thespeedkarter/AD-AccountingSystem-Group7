using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages
{
    [Authorize]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DashboardModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public int PendingJournalCount { get; set; }
        public int RejectedJournalCount { get; set; }
        public int ApprovedNotPostedCount { get; set; }
        public int AdjustingEntryCount { get; set; }

        public int TotalAccountCount { get; set; }
        public int ActiveAccountCount { get; set; }
        public int InactiveAccountCount { get; set; }

        public int PendingAccessRequestCount { get; set; }
        public int ExpiredPasswordCount { get; set; }
        public int TotalUserCount { get; set; }
        public int EmailOutboxCount { get; set; }

        public decimal CurrentRatio { get; set; }
        public decimal QuickRatio { get; set; }
        public decimal DebtRatio { get; set; }
        public decimal NetProfitMargin { get; set; }

        public List<JournalEntry> RecentEntries { get; set; } = new();

        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            PendingJournalCount = await _db.JournalEntries.CountAsync(j => j.Status == JournalStatus.Pending);
            RejectedJournalCount = await _db.JournalEntries.CountAsync(j => j.Status == JournalStatus.Rejected);
            ApprovedNotPostedCount = await _db.JournalEntries.CountAsync(j => j.Status == JournalStatus.Approved);
            AdjustingEntryCount = await _db.JournalEntries.CountAsync(j => j.IsAdjusting);

            TotalAccountCount = await _db.ChartAccounts.CountAsync();
            ActiveAccountCount = await _db.ChartAccounts.CountAsync(a => a.IsActive);
            InactiveAccountCount = await _db.ChartAccounts.CountAsync(a => !a.IsActive);

            PendingAccessRequestCount = await _db.AccessRequests.CountAsync(a => a.Status == AccessRequestStatus.Pending);
            ExpiredPasswordCount = await _db.UserSecurities.CountAsync(u => u.PasswordExpiresAt < DateTime.UtcNow);
            TotalUserCount = await _db.Users.CountAsync();
            EmailOutboxCount = await _db.SentEmails.CountAsync();

            var accounts = await _db.ChartAccounts
                .AsNoTracking()
                .Where(a => a.IsActive)
                .ToListAsync();

            var currentAssets = accounts
                .Where(a => a.Category == AccountCategory.Asset &&
                           (a.Subcategory == "Current Assets" || a.AccountNumber < 1500))
                .Sum(a => a.Balance);

            var quickAssets = accounts
                .Where(a => a.Category == AccountCategory.Asset &&
                           (a.Subcategory == "Current Assets" || a.AccountNumber < 1500) &&
                           a.AccountName != "Prepaid Expenses" &&
                           a.AccountName != "Supplies Inventory")
                .Sum(a => a.Balance);

            var totalAssets = accounts
                .Where(a => a.Category == AccountCategory.Asset)
                .Sum(a => a.Balance);

            var currentLiabilities = accounts
                .Where(a => a.Category == AccountCategory.Liability &&
                           (a.Subcategory == "Current Liabilities" || a.AccountNumber < 3000))
                .Sum(a => a.Balance);

            var totalLiabilities = accounts
                .Where(a => a.Category == AccountCategory.Liability)
                .Sum(a => a.Balance);

            var revenue = accounts
                .Where(a => a.Category == AccountCategory.Revenue)
                .Sum(a => a.Balance != 0 ? a.Balance : a.Credit);

            var expenses = accounts
                .Where(a => a.Category == AccountCategory.Expense)
                .Sum(a => a.Balance != 0 ? a.Balance : a.Debit);

            CurrentRatio = currentLiabilities == 0 ? 0 : currentAssets / currentLiabilities;
            QuickRatio = currentLiabilities == 0 ? 0 : quickAssets / currentLiabilities;
            DebtRatio = totalAssets == 0 ? 0 : totalLiabilities / totalAssets;
            NetProfitMargin = revenue == 0 ? 0 : (revenue - expenses) / revenue;

            RecentEntries = await _db.JournalEntries
                .AsNoTracking()
                .OrderByDescending(j => j.CreatedAtUtc)
                .Take(5)
                .ToListAsync();
        }

        public string GetHighGoodRatioClass(decimal value, decimal goodThreshold, decimal warningThreshold)
        {
            if (value >= goodThreshold) return "bg-success text-white";
            if (value >= warningThreshold) return "bg-warning text-dark";
            return "bg-danger text-white";
        }

        public string GetLowGoodRatioClass(decimal value, decimal goodThreshold, decimal warningThreshold)
        {
            if (value <= goodThreshold) return "bg-success text-white";
            if (value <= warningThreshold) return "bg-warning text-dark";
            return "bg-danger text-white";
        }

        public string GetCurrentRatioLabel()
        {
            if (CurrentRatio >= 2.0m) return "Healthy";
            if (CurrentRatio >= 1.0m) return "Warning";
            return "Needs Attention";
        }

        public string GetQuickRatioLabel()
        {
            if (QuickRatio >= 1.5m) return "Healthy";
            if (QuickRatio >= 1.0m) return "Warning";
            return "Needs Attention";
        }

        public string GetDebtRatioLabel()
        {
            if (DebtRatio <= 0.50m) return "Healthy";
            if (DebtRatio <= 0.65m) return "Warning";
            return "Needs Attention";
        }

        public string GetNetProfitMarginLabel()
        {
            if (NetProfitMargin >= 0.20m) return "Healthy";
            if (NetProfitMargin >= 0.10m) return "Warning";
            return "Needs Attention";
        }

        public string GetStatusBadgeClass(JournalStatus status)
        {
            return status switch
            {
                JournalStatus.Pending => "bg-warning text-dark",
                JournalStatus.Approved => "bg-primary",
                JournalStatus.Rejected => "bg-secondary",
                JournalStatus.Posted => "bg-success",
                _ => "bg-light text-dark"
            };
        }
    }
}