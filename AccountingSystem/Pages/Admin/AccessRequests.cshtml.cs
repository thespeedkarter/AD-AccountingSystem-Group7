using System.Text.Json;
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

            var adminUserId = _userManager.GetUserId(User);

            var before = SnapshotAccessRequest(req);

            // Create username: first initial + last name + MMYY
            var mmyy = DateTime.Now.ToString("MMyy");
            var username = $"{req.FirstName.Substring(0, 1)}{req.LastName}{mmyy}".ToLower();

            // Temp password (you can replace later with reset link flow)
            var tempPassword = "Temp!234";

            var user = new IdentityUser
            {
                UserName = username,
                Email = req.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, tempPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);

                await OnGetAsync();
                return Page();
            }

            await _userManager.AddToRoleAsync(user, RoleToAssign);

            req.Status = AccessRequestStatus.Approved;
            req.AdminComment = (AdminComment ?? "").Trim();
            req.ProcessedAt = DateTime.UtcNow;
            req.ProcessedByUserId = adminUserId;

            var after = SnapshotAccessRequest(req);

            // Log the access request approval + user creation info
            _db.EventLogs.Add(new EventLog
            {
                TableName = "AccessRequests",
                RecordId = req.AccessRequestId,
                Action = (int)EventAction.Approve,
                BeforeJson = JsonSerializer.Serialize(before),
                AfterJson = JsonSerializer.Serialize(new
                {
                    AccessRequest = after,
                    CreatedUser = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        RoleAssigned = RoleToAssign,
                        TempPassword = tempPassword
                    }
                }),
                UserId = adminUserId,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync()
        {
            var req = await _db.AccessRequests.FindAsync(RequestId);
            if (req == null) return NotFound();

            if (req.Status != AccessRequestStatus.Pending)
                return RedirectToPage();

            var adminUserId = _userManager.GetUserId(User);

            var before = SnapshotAccessRequest(req);

            req.Status = AccessRequestStatus.Rejected;
            req.AdminComment = (AdminComment ?? "").Trim();
            req.ProcessedAt = DateTime.UtcNow;
            req.ProcessedByUserId = adminUserId;

            var after = SnapshotAccessRequest(req);

            _db.EventLogs.Add(new EventLog
            {
                TableName = "AccessRequests",
                RecordId = req.AccessRequestId,
                Action = (int)EventAction.Reject,
                BeforeJson = JsonSerializer.Serialize(before),
                AfterJson = JsonSerializer.Serialize(after),
                UserId = adminUserId,
                CreatedAtUtc = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToPage();
        }

        private static object SnapshotAccessRequest(AccessRequest r) => new
        {
            r.AccessRequestId,
            r.FirstName,
            r.LastName,
            r.Email,
            r.DateOfBirth,
            r.Address,
            r.RequestedAt,
            r.Status,
            r.AdminComment,
            r.ProcessedByUserId,
            r.ProcessedAt
        };
    }
}