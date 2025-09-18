namespace Auth.API.Features.Authentication.ServiceContracts.Features;

public interface IUserAccountService
{
    Task<bool> UnlockAccountAsync(string email);
}