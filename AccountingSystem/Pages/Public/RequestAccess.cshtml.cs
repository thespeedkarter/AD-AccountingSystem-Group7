using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AccountingSystem.Pages.Public
{
    public class RequestAccessModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public RequestAccessModel(ApplicationDbContext db, IEmailSender emailSender)
        {
            _db = db;
            _emailSender = emailSender;
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

            // "Send email" to admin (actually saved to SentEmails table)
            await _emailSender.SendEmailAsync(
                "admin@local.test",
                "New Access Request Pending",
                $@"
                <p>A new access request has been submitted.</p>

                <p>
                <b>Name:</b> {Input.FirstName} {Input.LastName}<br>
                <b>Email:</b> {Input.Email}<br>
                <b>Address:</b> {Input.Address}<br>
                <b>DOB:</b> {Input.DateOfBirth:yyyy-MM-dd}
                </p>

                <p>Please login and review it under <b>Admin ? Access Requests</b>.</p>
                "
            );

            SuccessMessage = "Request submitted! An administrator will review it and email you if approved.";
            ModelState.Clear();
            Input = new();

            return Page();
        }
    }
}