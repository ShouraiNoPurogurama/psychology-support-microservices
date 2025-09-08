namespace Auth.API.Domains.Authentication.ServiceContracts.Features.v4;

public interface IUserAccountService
{
    Task<bool> UnlockAccountAsync(string email);
}