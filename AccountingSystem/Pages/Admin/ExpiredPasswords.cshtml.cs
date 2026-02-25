using AccountingSystem.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class ExpiredPasswordsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public ExpiredPasswordsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public class Row
        {
            public string Username { get; set; } = "";
            public string Email { get; set; } = "";
            public DateTime ExpiresAtUtc { get; set; }
        }

        public List<Row> Expired { get; set; } = new();

        public async Task OnGetAsync()
        {
            var now = DateTime.UtcNow;

            Expired = await (from sec in _db.UserSecurities
                             join u in _db.Users on sec.UserId equals u.Id
                             where sec.PasswordExpiresAt < now
                             orderby sec.PasswordExpiresAt
                             select new Row
                             {
                                 Username = u.UserName ?? "",
                                 Email = u.Email ?? "",
                                 ExpiresAtUtc = sec.PasswordExpiresAt
                             }).ToListAsync();
        }
    }
}
