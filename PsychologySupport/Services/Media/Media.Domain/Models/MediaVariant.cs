using Media.Domain.Enums;
using Media.Domain.Exceptions;

namespace Media.Domain.Models;

public sealed class MediaVariant : AuditableEntity<Guid>
{
    public Guid MediaId { get; private set; }
    public VariantType VariantType { get; private set; }
    public MediaFormat Format { get; private set; }
    public int Width { get; private set; }
    public int Height { get; private set; }
    public long Bytes { get; private set; }
    public string BucketKey { get; private set; } = null!;
    public string? CdnUrl { get; private set; }

    public MediaAsset Media { get; private set; } = null!;

    // EF Core constructor
    private MediaVariant() { }

    // Factory method
    public static MediaVariant Create(
        Guid mediaId, 
        VariantType variantType, 
        MediaFormat format, 
        int width, 
        int height, 
        long bytes, 
        string bucketKey, 
        string? cdnUrl = null)
    {
        if (width <= 0)
            throw new MediaDomainException("Width must be greater than zero.");
        
        if (height <= 0)
            throw new MediaDomainException("Height must be greater than zero.");
        
        if (bytes <= 0)
            throw new MediaDomainException("Bytes must be greater than zero.");
        
        if (string.IsNullOrWhiteSpace(bucketKey))
            throw new MediaDomainException("Bucket key cannot be empty.");

        return new MediaVariant
        {
            Id = Guid.NewGuid(),
            MediaId = mediaId,
            VariantType = variantType,
            Format = format,
            Width = width,
            Height = height,
            Bytes = bytes,
            BucketKey = bucketKey,
            CdnUrl = cdnUrl,
            CreatedAt = DateTime.UtcNow
        };
    }

    // Business methods
    public void UpdateCdnUrl(string cdnUrl)
    {
        if (string.IsNullOrWhiteSpace(cdnUrl))
            throw new MediaDomainException("CDN URL cannot be empty.");

        CdnUrl = cdnUrl;
    }

    public void ClearCdnUrl()
    {
        CdnUrl = null;
    }

    // Query properties
    public bool IsOriginal => VariantType == VariantType.Original;
    public bool IsThumbnail => VariantType == VariantType.Thumbnail;
    public bool HasCdnUrl => !string.IsNullOrWhiteSpace(CdnUrl);
    public long PixelCount => (long)Width * Height;
    public double AspectRatio => (double)Width / Height;

    public string SizeDisplay => Bytes switch
    {
        < 1024 => $"{Bytes} B",
        < 1024 * 1024 => $"{Bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{Bytes / (1024.0 * 1024):F1} MB",
        _ => $"{Bytes / (1024.0 * 1024 * 1024):F1} GB"
    };

    public string DimensionsDisplay => $"{Width}x{Height}";
}
