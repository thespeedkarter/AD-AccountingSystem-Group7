using System.Text.Json;
using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Admin.EventLogs
{
    [Authorize(Roles = "Administrator,Manager")]
    public class ViewModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public ViewModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public EventLog Log { get; set; } = default!;

        public string PrettyBefore { get; set; } = "";
        public string PrettyAfter { get; set; } = "";

        public string ActionLabel(int action) => action switch
        {
            1 => "Create",
            2 => "Update",
            3 => "Delete",
            10 => "Approve",
            11 => "Reject",
            20 => "Post",
            30 => "Upload",
            _ => action.ToString()
        };

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var log = await _db.EventLogs.AsNoTracking().FirstOrDefaultAsync(x => x.EventLogId == id);
            if (log == null) return NotFound();

            Log = log;

            PrettyBefore = PrettyJson(Log.BeforeJson);
            PrettyAfter = PrettyJson(Log.AfterJson);

            return Page();
        }

        private static string PrettyJson(string? json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "";

            try
            {
                using var doc = JsonDocument.Parse(json);
                return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                // If it isn't valid JSON for some reason, just show raw
                return json;
            }
        }
    }
}