using Auth.API.Domains.Authentication.Exceptions;
using Auth.API.Domains.Authentication.ServiceContracts.Features.v4;

namespace Auth.API.Domains.Authentication.Services.Features.v4;

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