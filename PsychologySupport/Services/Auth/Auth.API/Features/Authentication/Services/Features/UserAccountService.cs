using Auth.API.Data;
using Auth.API.Features.Authentication.Exceptions;
using Auth.API.Features.Authentication.ServiceContracts.Features;

namespace Auth.API.Features.Authentication.Services.Features;

public class UserAccountService(UserManager<User> userManager, AuthDbContext dbContext) : IUserAccountService
{
    public async Task<bool> UnlockAccountAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email)
                   ?? throw new UserNotFoundException(email);

        await userManager.SetLockoutEnabledAsync(user, false);
        await userManager.SetLockoutEndDateAsync(user, null);

        return true;
    }
}