using Media.Domain.Exceptions;

namespace Media.Domain.ValueObjects;

public sealed record MediaDimensions
{
    public int Width { get; init; }
    public int Height { get; init; }
    public long AspectRatioNumerator { get; private init; }
    public long AspectRatioDenominator { get; private init; }

    private MediaDimensions() { }

    private MediaDimensions(int width, int height)
    {
        Width = width;
        Height = height;
        
        // Calculate GCD for aspect ratio
        var gcd = CalculateGcd(width, height);
        AspectRatioNumerator = width / gcd;
        AspectRatioDenominator = height / gcd;
    }

    public static MediaDimensions Create(int width, int height)
    {
        if (width <= 0)
            throw new MediaDomainException("Width must be greater than zero.");

        if (height <= 0)
            throw new MediaDomainException("Height must be greater than zero.");

        if (width > 50000 || height > 50000)
            throw new MediaDomainException("Dimensions cannot exceed 50,000 pixels.");

        return new MediaDimensions(width, height);
    }

    public static MediaDimensions? CreateOptional(int? width, int? height)
    {
        if (!width.HasValue || !height.HasValue)
            return null;

        return Create(width.Value, height.Value);
    }

    public bool IsLandscape => Width > Height;
    public bool IsPortrait => Height > Width;
    public bool IsSquare => Width == Height;
    public long PixelCount => (long)Width * Height;

    public string AspectRatioDisplay => $"{AspectRatioNumerator}:{AspectRatioDenominator}";

    private static int CalculateGcd(int a, int b)
    {
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }
}
