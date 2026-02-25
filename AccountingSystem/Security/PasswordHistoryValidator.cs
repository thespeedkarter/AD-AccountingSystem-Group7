using AccountingSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AccountingSystem.Security
{
    public class PasswordHistoryValidator : IPasswordValidator<IdentityUser>
    {
        private readonly ApplicationDbContext _db;

        public PasswordHistoryValidator(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user, string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return IdentityResult.Failed(new IdentityError { Description = "Password is required." });

            // Check last 5 passwords
            var history = await _db.PasswordHistories
                .Where(h => h.UserId == user.Id)
                .OrderByDescending(h => h.CreatedAt)
                .Take(5)
                .ToListAsync();

            foreach (var item in history)
            {
                // Verify candidate password against each previous hash
                var verify = manager.PasswordHasher.VerifyHashedPassword(user, item.PasswordHash, password);
                if (verify == PasswordVerificationResult.Success)
                {
                    return IdentityResult.Failed(new IdentityError
                    {
                        Description = "You cannot reuse a previously used password."
                    });
                }
            }

            return IdentityResult.Success;
        }
    }
}
