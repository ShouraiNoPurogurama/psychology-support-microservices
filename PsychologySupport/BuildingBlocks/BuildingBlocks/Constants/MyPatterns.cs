namespace BuildingBlocks.Constants;

public static class MyPatterns
{
    /// <summary>
    /// Password must be at least 8 characters long and contain at least one digit, one lowercase letter, one uppercase letter.
    /// </summary>
    public static readonly string Password = "(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$";
    
    /// <summary>
    /// Phone number must has at 10-12 digits
    /// </summary>
    public static readonly string PhoneNumber = @"^\+?[0-9]{10,12}$";
    
    public static readonly string Email = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
}