using System.Text.Json;
using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.ChartOfAccounts
{
    [Authorize(Roles = "Administrator,Manager,Accountant")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public EditModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public ChartAccount Input { get; set; } = new();

        [BindProperty]
        public string EmailToRole { get; set; } = "Manager";

        [BindProperty]
        public string? EmailSubject { get; set; }

        [BindProperty]
        public string? EmailBody { get; set; }

        [TempData] public string? StatusMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var acct = await _db.ChartAccounts.FirstOrDefaultAsync(a => a.ChartAccountId == id);
            if (acct == null) return NotFound();

            Input = acct;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!User.IsInRole("Administrator"))
                return Forbid();

            var existing = await _db.ChartAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ChartAccountId == Input.ChartAccountId);

            if (existing == null) return NotFound();

            if (!ModelState.IsValid)
                return Page();

            if (await _db.ChartAccounts.AnyAsync(a =>
                    a.ChartAccountId != Input.ChartAccountId &&
                    a.AccountNumber == Input.AccountNumber))
            {
                ModelState.AddModelError(string.Empty, "Duplicate account number is not allowed.");
            }

            if (await _db.ChartAccounts.AnyAsync(a =>
                    a.ChartAccountId != Input.ChartAccountId &&
                    a.AccountName == Input.AccountName))
            {
                ModelState.AddModelError(string.Empty, "Duplicate account name is not allowed.");
            }

            if (!ModelState.IsValid)
                return Page();

            _db.ChartAccounts.Update(Input);
            await _db.SaveChangesAsync();

            _db.EventLogs.Add(new EventLog
            {
                TableName = "ChartAccounts",
                RecordId = Input.ChartAccountId,
                Action = (int)EventAction.Update,
                BeforeJson = JsonSerializer.Serialize(existing),
                AfterJson = JsonSerializer.Serialize(Input),
                UserId = _userManager.GetUserId(User),
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            StatusMessage = "Account updated.";
            return RedirectToPage("./Edit", new { id = Input.ChartAccountId });
        }

        public async Task<IActionResult> OnPostDeactivateAsync()
        {
            if (!User.IsInRole("Administrator"))
                return Forbid();

            var acct = await _db.ChartAccounts.FirstOrDefaultAsync(a => a.ChartAccountId == Input.ChartAccountId);
            if (acct == null) return NotFound();

            if (acct.Balance > 0)
            {
                ErrorMessage = "Accounts with balance greater than zero cannot be deactivated.";
                return RedirectToPage("./Edit", new { id = acct.ChartAccountId });
            }

            var before = JsonSerializer.Serialize(acct);

            acct.IsActive = false;
            await _db.SaveChangesAsync();

            _db.EventLogs.Add(new EventLog
            {
                TableName = "ChartAccounts",
                RecordId = acct.ChartAccountId,
                Action = (int)EventAction.Deactivate,
                BeforeJson = before,
                AfterJson = JsonSerializer.Serialize(acct),
                UserId = _userManager.GetUserId(User),
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            StatusMessage = "Account deactivated.";
            return RedirectToPage("./Edit", new { id = acct.ChartAccountId });
        }

        public async Task<IActionResult> OnPostSendEmailAsync()
        {
            var acct = await _db.ChartAccounts
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.ChartAccountId == Input.ChartAccountId);

            if (acct == null) return NotFound();

            if (string.IsNullOrWhiteSpace(EmailSubject) || string.IsNullOrWhiteSpace(EmailBody))
            {
                ErrorMessage = "Subject and message are required.";
                return RedirectToPage("./Edit", new { id = Input.ChartAccountId });
            }

            var role = (EmailToRole ?? "").Trim();
            if (role != "Administrator" && role != "Manager" && role != "Accountant")
            {
                ErrorMessage = "Invalid recipient role.";
                return RedirectToPage("./Edit", new { id = Input.ChartAccountId });
            }

            var recipients = await (
                from u in _db.Users
                join ur in _db.UserRoles on u.Id equals ur.UserId
                join r in _db.Roles on ur.RoleId equals r.Id
                where r.Name == role
                select new { u.Id, u.Email }
            ).ToListAsync();

            if (recipients.Count == 0)
            {
                ErrorMessage = $"No users found in role '{role}'.";
                return RedirectToPage("./Edit", new { id = Input.ChartAccountId });
            }

            foreach (var rec in recipients.Where(x => !string.IsNullOrWhiteSpace(x.Email)))
            {
                _db.SentEmails.Add(new SentEmail
                {
                    ToEmail = rec.Email!,
                    ToUserId = rec.Id,
                    Subject = EmailSubject.Trim(),
                    BodyHtml =
                        $@"<p><strong>Account:</strong> {acct.AccountNumber} - {acct.AccountName}</p>
                           <p>{System.Net.WebUtility.HtmlEncode(EmailBody).Replace("\n", "<br/>")}</p>",
                    SentByUserId = _userManager.GetUserId(User),
                    SentAtUtc = DateTime.UtcNow,
                    Channel = "OutboxDb"
                });
            }

            await _db.SaveChangesAsync();

            StatusMessage = $"Email logged to {role} user(s).";
            return RedirectToPage("./Edit", new { id = Input.ChartAccountId });
        }
    }
}