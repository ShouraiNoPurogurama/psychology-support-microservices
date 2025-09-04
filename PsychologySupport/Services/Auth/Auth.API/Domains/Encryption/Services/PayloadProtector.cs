using System.Text.Json;
using Auth.API.Domains.Encryption.ServiceContracts;
using Microsoft.AspNetCore.DataProtection;

namespace Auth.API.Domains.Encryption.Services;

public class PayloadProtector : IPayloadProtector
{
    private readonly IDataProtector _protector;
    public PayloadProtector(IDataProtectionProvider provider)
    {
        //purpose riêng cho pending seed
        _protector = provider.CreateProtector("Auth.PendingProfileSeed.v1");
    }

    public byte[] Protect<T>(T model)
        => _protector.Protect(JsonSerializer.SerializeToUtf8Bytes(model));

    public T Unprotect<T>(byte[] blob)
        => JsonSerializer.Deserialize<T>(_protector.Unprotect(blob))!;
}