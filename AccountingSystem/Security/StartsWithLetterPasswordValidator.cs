using Microsoft.AspNetCore.Identity;

namespace AccountingSystem.Security
{
    public class StartsWithLetterPasswordValidator : IPasswordValidator<IdentityUser>
    {
        public Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user, string? password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Password is required." }));

            if (!char.IsLetter(password[0]))
                return Task.FromResult(IdentityResult.Failed(new IdentityError { Description = "Password must start with a letter." }));

            return Task.FromResult(IdentityResult.Success);
        }
    }
}
