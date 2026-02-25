using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.ChartOfAccounts
{
    [Authorize] // managers/admins/accountants can view logs if you want
    public class LogsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public LogsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public List<EventLog> Logs { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? RecordId { get; set; }

        public async Task OnGetAsync()
        {
            IQueryable<EventLog> q = _db.EventLogs.AsNoTracking()
                .Where(x => x.TableName == "ChartAccounts");

            if (RecordId.HasValue)
                q = q.Where(x => x.RecordId == RecordId.Value);

            Logs = await q
                .OrderByDescending(x => x.CreatedAtUtc)
                .Take(200)
                .ToListAsync();
        }
    }
}
