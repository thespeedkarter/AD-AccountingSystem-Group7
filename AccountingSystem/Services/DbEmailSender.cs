using System.Threading.Tasks;
using AccountingSystem.Data;
using AccountingSystem.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Services
{
    // Implements Identity UI email sender so Register/ForgotPassword/etc won't crash
    public class DbEmailSender : IEmailSender
    {
        private readonly ApplicationDbContext _db;

        public DbEmailSender(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Try to map email -> userId if possible (nice for reporting)
            string? userId = null;
            try
            {
                userId = await _db.Users
                    .Where(u => u.Email == email)
                    .Select(u => u.Id)
                    .FirstOrDefaultAsync();
            }
            catch
            {
                // ignore
            }

            _db.SentEmails.Add(new SentEmail
            {
                ToEmail = email,
                ToUserId = userId,
                Subject = subject,
                BodyHtml = htmlMessage,
                Channel = "OutboxDb"
            });

            await _db.SaveChangesAsync();
        }
    }
}