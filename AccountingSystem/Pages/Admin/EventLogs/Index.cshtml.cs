using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Admin.EventLogs
{
    [Authorize(Roles = "Administrator")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<EventLog> Logs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Table { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Action { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? From { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? To { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        // DO NOT name this "Page"
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; } = 25;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            if (PageNumber < 1) PageNumber = 1;

            IQueryable<EventLog> q = _db.EventLogs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(Table))
                q = q.Where(x => x.TableName == Table);

            if (Action.HasValue)
                q = q.Where(x => x.Action == Action.Value);

            if (From.HasValue)
                q = q.Where(x => x.CreatedAtUtc >= From.Value.Date);

            if (To.HasValue)
                q = q.Where(x => x.CreatedAtUtc < To.Value.Date.AddDays(1));

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                q = q.Where(x =>
                    x.TableName.Contains(s) ||
                    (x.UserId != null && x.UserId.Contains(s)) ||
                    (x.BeforeJson != null && x.BeforeJson.Contains(s)) ||
                    (x.AfterJson != null && x.AfterJson.Contains(s)));
            }

            TotalCount = await q.CountAsync();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            if (TotalPages > 0 && PageNumber > TotalPages) PageNumber = TotalPages;

            Logs = await q
                .OrderByDescending(x => x.CreatedAtUtc)
                .ThenByDescending(x => x.EventLogId)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}