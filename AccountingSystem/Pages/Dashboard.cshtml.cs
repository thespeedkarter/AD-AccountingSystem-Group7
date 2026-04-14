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
        }
    }
}