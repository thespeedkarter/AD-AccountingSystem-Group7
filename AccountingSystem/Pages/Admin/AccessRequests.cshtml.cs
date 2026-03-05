using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class AccessRequestsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public AccessRequestsModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<AccessRequest> Requests { get; set; } = new();

        [BindProperty] public int RequestId { get; set; }
        [BindProperty] public string? AdminComment { get; set; }
        [BindProperty] public string RoleToAssign { get; set; } = "Accountant";

        [TempData] public string? StatusMessage { get; set; }
        [TempData] public string? ErrorMessage { get; set; }

        public async Task OnGetAsync()
        {
            Requests = await _db.AccessRequests
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostApproveAsync()
        {
            var req = await _db.AccessRequests.FindAsync(RequestId);
            if (req == null) return NotFound();

            if (req.Status != AccessRequestStatus.Pending)
                return RedirectToPage();

            // Username rule: first initial + last name + MMYY
            var mmyy = DateTime.Now.ToString("MMyy");
            var username = $"{req.FirstName.Substring(0, 1)}{req.LastName}{mmyy}".ToLower();

            // Temp password (Sprint 1 simple approach)
            var tempPassword = "Temp!234";

            var user = new IdentityUser
            {
                UserName = username,
                Email = req.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, tempPassword);
            // Save initial password hash into history
            if (!string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                _db.PasswordHistories.Add(new PasswordHistory
                {
                    UserId = user.Id,
                    PasswordHash = user.PasswordHash,
                    CreatedAt = DateTime.UtcNow
                });
            }
            if (!createResult.Succeeded)
            {
                ErrorMessage = string.Join(" | ", createResult.Errors.Select(e => e.Description));
                return RedirectToPage();
            }

            // assign role
            await _userManager.AddToRoleAsync(user, RoleToAssign);

            // Ensure UserSecurity row exists
            var sec = await _db.UserSecurities.FirstOrDefaultAsync(x => x.UserId == user.Id);
            if (sec == null)
            {
                sec = new UserSecurity
                {
                    UserId = user.Id,
                    IsActive = true,
                    PasswordLastChangedAt = DateTime.UtcNow,
                    PasswordExpiresAt = DateTime.UtcNow.AddDays(90)
                };
                _db.UserSecurities.Add(sec);
            }

            // mark request approved
            req.Status = AccessRequestStatus.Approved;
            req.ProcessedAt = DateTime.UtcNow;
            req.ProcessedByUserId = _userManager.GetUserId(User);

            var note = $"Approved | Username: {username} | Temp password: {tempPassword}";
            req.AdminComment = string.IsNullOrWhiteSpace(AdminComment)
                ? note
                : $"{AdminComment.Trim()} | {note}";

            // "Email" (Option A: log to SentEmails table)
            var loginUrl = $"{Request.Scheme}://{Request.Host}/Identity/Account/Login";
            _db.SentEmails.Add(new SentEmail
            {
                ToEmail = req.Email,
                ToUserId = user.Id,
                Subject = "Your AccountingSystem access has been approved",
                BodyHtml =
                    $@"<p>Hello {req.FirstName},</p>
                       <p>Your access request has been <strong>approved</strong>.</p>
                       <p><strong>Username:</strong> {username}<br/>
                          <strong>Temporary Password:</strong> {tempPassword}</p>
                       <p>Login here: <a href=""{loginUrl}"">{loginUrl}</a></p>
                       <p>After login, please change your password under Manage Account.</p>",
                SentByUserId = _userManager.GetUserId(User),
                SentAtUtc = DateTime.UtcNow,
                Channel = "OutboxDb"
            });

            await _db.SaveChangesAsync();

            StatusMessage = $"Approved: {req.Email} (username: {username})";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync()
        {
            var req = await _db.AccessRequests.FindAsync(RequestId);
            if (req == null) return NotFound();

            if (req.Status != AccessRequestStatus.Pending)
                return RedirectToPage();

            var reason = string.IsNullOrWhiteSpace(AdminComment) ? null : AdminComment.Trim();
            if (string.IsNullOrWhiteSpace(reason))
            {
                ErrorMessage = "Rejecting requires a comment (reason).";
                return RedirectToPage();
            }

            req.Status = AccessRequestStatus.Rejected;
            req.AdminComment = reason;
            req.ProcessedAt = DateTime.UtcNow;
            req.ProcessedByUserId = _userManager.GetUserId(User);

            _db.SentEmails.Add(new SentEmail
            {
                ToEmail = req.Email,
                Subject = "Your AccountingSystem access request was rejected",
                BodyHtml =
                    $@"<p>Hello {req.FirstName},</p>
                       <p>Your access request has been <strong>rejected</strong>.</p>
                       <p><strong>Reason:</strong> {System.Net.WebUtility.HtmlEncode(reason)}</p>",
                SentByUserId = _userManager.GetUserId(User),
                SentAtUtc = DateTime.UtcNow,
                Channel = "OutboxDb"
            });

            await _db.SaveChangesAsync();

            StatusMessage = $"Rejected: {req.Email}";
            return RedirectToPage();
        }
    }
}