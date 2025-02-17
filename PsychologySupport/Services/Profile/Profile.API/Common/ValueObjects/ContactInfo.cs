namespace Profile.API.Common.ValueObjects;

public record ContactInfo
{
    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }
}