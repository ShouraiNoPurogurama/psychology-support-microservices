namespace Auth.API.Domains.Encryption.ServiceContracts;

public interface IPayloadProtector
{
    byte[] Protect<T>(T model);
    T Unprotect<T>(byte[] protectedBytes);
}