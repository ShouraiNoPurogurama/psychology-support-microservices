using Media.Domain.Exceptions;

namespace Media.Domain.ValueObjects;

public sealed record MediaContent
{
    public string MimeType { get; init; } = default!;
    public long SizeInBytes { get; init; }
    public string? Phash64 { get; init; }

    private MediaContent() { }

    private MediaContent(string mimeType, long sizeInBytes, string? phash64 = null)
    {
        MimeType = mimeType;
        SizeInBytes = sizeInBytes;
        Phash64 = phash64;
    }

    public static MediaContent Create(string mimeType, long sizeInBytes, string? phash64 = null)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            throw new MediaDomainException("MIME type cannot be empty.");

        if (!IsValidMimeType(mimeType))
            throw new MediaDomainException($"Unsupported MIME type: {mimeType}");

        if (sizeInBytes <= 0)
            throw new MediaDomainException("File size must be greater than zero.");

        if (sizeInBytes > MaxFileSizeInBytes)
            throw new MediaDomainException($"File size ({sizeInBytes} bytes) exceeds maximum allowed size ({MaxFileSizeInBytes} bytes).");

        return new MediaContent(mimeType, sizeInBytes, phash64);
    }

    public bool IsImage => MimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    public bool IsVideo => MimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase);
    public bool IsAudio => MimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase);

    public string FileExtension => MimeType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/gif" => ".gif",
        "image/webp" => ".webp",
        "video/mp4" => ".mp4",
        "video/webm" => ".webm",
        "audio/mpeg" => ".mp3",
        "audio/wav" => ".wav",
        _ => ""
    };

    public string SizeDisplay => SizeInBytes switch
    {
        < 1024 => $"{SizeInBytes} B",
        < 1024 * 1024 => $"{SizeInBytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{SizeInBytes / (1024.0 * 1024):F1} MB",
        _ => $"{SizeInBytes / (1024.0 * 1024 * 1024):F1} GB"
    };

    private const long MaxFileSizeInBytes = 100 * 1024 * 1024; // 100MB

    private static readonly HashSet<string> SupportedMimeTypes = new()
    {
        "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp",
        "video/mp4", "video/webm", "video/quicktime",
        "audio/mpeg", "audio/wav", "audio/ogg"
    };

    private static bool IsValidMimeType(string mimeType)
        => SupportedMimeTypes.Contains(mimeType.ToLowerInvariant());
}
