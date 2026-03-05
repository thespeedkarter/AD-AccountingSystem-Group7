using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class EmailOutboxDetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public EmailOutboxDetailsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public SentEmail Email { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var email = await _db.SentEmails.AsNoTracking().FirstOrDefaultAsync(x => x.SentEmailId == id);
            if (email == null) return NotFound();

            Email = email;
            return Page();
        }
    }
}