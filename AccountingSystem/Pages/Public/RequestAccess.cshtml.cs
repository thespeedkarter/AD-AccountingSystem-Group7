using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccountingSystem.Pages.Public
{
    public class RequestAccessModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public RequestAccessModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public AccessRequest Input { get; set; } = new();

        public string? SuccessMessage { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Always set server-side fields
            Input.RequestedAt = DateTime.UtcNow;
            Input.Status = AccessRequestStatus.Pending;

            _db.AccessRequests.Add(Input);
            await _db.SaveChangesAsync();

            SuccessMessage = "Request submitted! An administrator will review it and email you if approved.";
            ModelState.Clear();
            Input = new();

            return Page();
        }
    }
}
