using Auth.API.Domains.Authentication.Exceptions;
using Auth.API.Domains.Authentication.ServiceContracts.Features;

namespace Auth.API.Domains.Authentication.Services.Features;

public class UserAccountService(UserManager<User> userManager) : IUserAccountService
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