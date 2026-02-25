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

        public string? StatusMessage { get; set; }

        public async Task OnGetAsync()
        {
            PendingJournalCount = await _db.JournalEntries.CountAsync(j => j.Status == JournalStatus.Pending);
            RejectedJournalCount = await _db.JournalEntries.CountAsync(j => j.Status == JournalStatus.Rejected);
            ApprovedNotPostedCount = await _db.JournalEntries.CountAsync(j => j.Status == JournalStatus.Approved);
        }
    }
}