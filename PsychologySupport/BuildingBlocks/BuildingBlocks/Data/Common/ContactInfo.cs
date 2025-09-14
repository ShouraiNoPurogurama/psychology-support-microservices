using BuildingBlocks.Utils;

namespace BuildingBlocks.Data.Common;

public record ContactInfo
{
    public string? Address { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }

    private ContactInfo()
    {
    }

    private ContactInfo(string? address, string email, string? phoneNumber)
    {
        Address = address;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public static ContactInfo Of(string? address, string email, string? phoneNumber = null)
    {
        ValidationBuilder.Create()
            .When(() => string.IsNullOrWhiteSpace(email))
            .WithErrorCode("INVALID_EMAIL")
            .WithMessage("Email không được để trống.")
            .ThrowIfInvalid();

        return new ContactInfo(address, email, phoneNumber);
    }

    public static ContactInfo UpdateWithFallback(ContactInfo current, 
        string? address = null, 
        string? email = null,
        string? phoneNumber = null)
    {
        var finalAddress = address ?? current.Address;
        var finalEmail = email ?? current.Email;
        var finalPhoneNumber = phoneNumber ?? current.PhoneNumber;

        ValidationBuilder.Create()
            .When(() => string.IsNullOrWhiteSpace(finalEmail))
            .WithErrorCode("INVALID_EMAIL")
            .WithMessage("Email không được để trống.")
            .ThrowIfInvalid();

        return new ContactInfo(finalAddress, finalEmail, finalPhoneNumber);
    }

    public static ContactInfo Empty() => new ContactInfo(null, string.Empty, null);

    public bool HasEnoughInfo() =>
        !string.IsNullOrWhiteSpace(Email);
}