namespace BuildingBlocks.Data.Common;

public record ContactInfo
{
    public string Address { get; private set; } = "None"!;
    public string Email { get; private set; } = default!;
    public string? PhoneNumber { get; private set; }

    public ContactInfo()
    {
    }

    private ContactInfo(string address, string email, string? phoneNumber)
    {
        Address = address;
        Email = email;
        PhoneNumber = phoneNumber;
    }

    public static ContactInfo Of(string address, string email, string? phoneNumber = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(address, nameof(address));
        ArgumentException.ThrowIfNullOrWhiteSpace(email, nameof(email));

        return new ContactInfo(address, email, phoneNumber);
    }
    
    public static ContactInfo UpdateWithFallback(ContactInfo current, string? address, string? email, string? phoneNumber)
    {
        var finalAddress = address ?? current.Address;
        var finalEmail = email ?? current.Email;

        ArgumentException.ThrowIfNullOrWhiteSpace(finalAddress, nameof(finalAddress));
        ArgumentException.ThrowIfNullOrWhiteSpace(finalEmail, nameof(finalEmail));

        return new ContactInfo(finalAddress, finalEmail, phoneNumber ?? current.PhoneNumber);
    }


    public static ContactInfo Empty() => new ContactInfo(string.Empty, string.Empty, null);

    public bool HasEnoughInfo() => 
        !string.IsNullOrWhiteSpace(Address) &&
        !string.IsNullOrWhiteSpace(Email);
}