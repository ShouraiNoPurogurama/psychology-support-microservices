namespace Profile.API.Common.ValueObjects;

public record ContactInfo
{
    public string? Address { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Email { get; set; }

    public ContactInfo()
    {
        
    }

    public ContactInfo(string? email, string? phoneNumber, string? address)
    {
        Email = email;
        PhoneNumber = phoneNumber;
        Address = address;
    }
}