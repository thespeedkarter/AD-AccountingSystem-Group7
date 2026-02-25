using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Journal
{
    [Authorize(Roles = "Manager")]
    public class ApproveModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public ApproveModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public JournalEntry Entry { get; set; } = default!;

        [BindProperty]
        public string? ManagerComment { get; set; }

        // Calculated helpers for the view
        public decimal TotalDebit { get; set; }
        public decimal TotalCredit { get; set; }
        public bool IsBalanced => TotalDebit == TotalCredit;

        public bool CanApproveOrReject =>
            Entry != null &&
            Entry.Status == JournalStatus.Pending &&
            IsBalanced;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var entry = await _db.JournalEntries
                .Include(e => e.Lines)
                .ThenInclude(l => l.ChartAccount)
                .FirstOrDefaultAsync(e => e.JournalEntryId == id);

            if (entry == null) return NotFound();

            Entry = entry;
            TotalDebit = Entry.Lines.Sum(l => l.Debit);
            TotalCredit = Entry.Lines.Sum(l => l.Credit);

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            var entry = await _db.JournalEntries
                .Include(e => e.Lines)
                .FirstOrDefaultAsync(e => e.JournalEntryId == id);

            if (entry == null) return NotFound();

            var totalD = entry.Lines.Sum(l => l.Debit);
            var totalC = entry.Lines.Sum(l => l.Credit);

            if (entry.Status != JournalStatus.Pending)
                return RedirectToPage("./Details", new { id });

            if (totalD != totalC)
            {
                ModelState.AddModelError(string.Empty, "Cannot approve: debits and credits do not balance.");
                return await OnGetAsync(id);
            }

            entry.Status = JournalStatus.Approved;
            entry.ApprovedByUserId = _userManager.GetUserId(User);
            entry.ApprovedAtUtc = DateTime.UtcNow;
            entry.ManagerComment = null;

            await _db.SaveChangesAsync();
            return RedirectToPage("./Details", new { id });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id)
        {
            var entry = await _db.JournalEntries.FirstOrDefaultAsync(e => e.JournalEntryId == id);
            if (entry == null) return NotFound();

            if (entry.Status != JournalStatus.Pending)
                return RedirectToPage("./Details", new { id });

            if (string.IsNullOrWhiteSpace(ManagerComment))
            {
                ModelState.AddModelError(string.Empty, "Rejecting requires a reason in the comment field.");
                return await OnGetAsync(id);
            }

            entry.Status = JournalStatus.Rejected;
            entry.ManagerComment = ManagerComment.Trim();
            entry.ApprovedByUserId = _userManager.GetUserId(User);
            entry.ApprovedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return RedirectToPage("./Details", new { id });
        }
    }
}