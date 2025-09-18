namespace Auth.API.Features.Encryption.ServiceContracts;

public interface IPayloadProtector
{
    byte[] Protect<T>(T model);
    T Unprotect<T>(byte[] protectedBytes);
}