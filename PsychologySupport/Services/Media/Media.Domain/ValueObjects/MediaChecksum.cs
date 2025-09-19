using Media.Domain.Exceptions;

namespace Media.Domain.ValueObjects;

public sealed record MediaChecksum
{
    public string Value { get; init; } = default!;
    public string Algorithm { get; init; } = default!;

    private MediaChecksum() { }

    private MediaChecksum(string value, string algorithm)
    {
        Value = value;
        Algorithm = algorithm;
    }

    public static MediaChecksum CreateSha256(string checksumValue)
    {
        if (string.IsNullOrWhiteSpace(checksumValue))
            throw new MediaDomainException("Checksum value cannot be empty.");

        if (checksumValue.Length != 44) // Base64 encoded SHA256 length
            throw new MediaDomainException("Invalid SHA256 checksum format.");

        return new MediaChecksum(checksumValue, "SHA256");
    }

    public static MediaChecksum Create(string value, string algorithm)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new MediaDomainException("Checksum value cannot be empty.");

        if (string.IsNullOrWhiteSpace(algorithm))
            throw new MediaDomainException("Checksum algorithm cannot be empty.");

        return new MediaChecksum(value, algorithm);
    }

    public bool Equals(MediaChecksum other)
        => Value == other?.Value && Algorithm == other?.Algorithm;

    public override int GetHashCode()
        => HashCode.Combine(Value, Algorithm);
}
