using System.Security.Cryptography;

namespace Auth.API.Domains.Authentication.Utils;

public static class DeterministicGuid
{
    private static readonly Guid SubjectRefNamespace =
        Guid.Parse("f25a9770-8ce4-4fe9-bed3-b1dc4a2833fb");

    private static readonly Guid ProfileIdNamespace =
        Guid.Parse("7a5c2e8d-05a7-4b67-a22b-6f1b1c987654");

    public static Guid SubjectRefFromUserId(Guid userId)
        => Generate(userId, SubjectRefNamespace);

    public static Guid ProfileIdFromUserId(Guid userId)
        => Generate(userId, ProfileIdNamespace);

    private static Guid Generate(Guid userId, Guid ns)
    {
        var nameBytes = userId.ToByteArray();
        var nsBytes = ns.ToByteArray();
        var data = new byte[nsBytes.Length + nameBytes.Length];
        Buffer.BlockCopy(nsBytes, 0, data, 0, nsBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, data, nsBytes.Length, nameBytes.Length);

        using var sha1 = SHA1.Create();
        var hash = sha1.ComputeHash(data);

        var newGuid = new byte[16];
        Array.Copy(hash, 0, newGuid, 0, 16);

        //version 5, variant RFC 4122
        newGuid[6] = (byte)((newGuid[6] & 0x0F) | (5 << 4));
        newGuid[8] = (byte)((newGuid[8] & 0x3F) | 0x80);

        return new Guid(newGuid);
    }
}