using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class EmailOutboxModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public EmailOutboxModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<SentEmail> Emails { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        public async Task OnGetAsync()
        {
            IQueryable<SentEmail> q = _db.SentEmails.AsNoTracking();

            if (From.HasValue) q = q.Where(x => x.SentAtUtc >= From.Value.Date);
            if (To.HasValue) q = q.Where(x => x.SentAtUtc <= To.Value.Date.AddDays(1).AddTicks(-1));

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                q = q.Where(x => x.ToEmail.Contains(s) || x.Subject.Contains(s));
            }

            Emails = await q
                .OrderByDescending(x => x.SentAtUtc)
                .Take(250)
                .ToListAsync();
        }
    }
}