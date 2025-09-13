namespace Auth.API.Domains.Authentication.ServiceContracts.Features;

public interface IUserAccountService
{
    Task<bool> UnlockAccountAsync(string email);
}