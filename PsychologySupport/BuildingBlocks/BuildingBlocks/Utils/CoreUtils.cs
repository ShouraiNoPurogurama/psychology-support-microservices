namespace BuildingBlocks.Utils;

public class CoreUtils
{
    public static DateTimeOffset SystemTimeNow => TimeUtils.ConvertToUtcPlus7(DateTimeOffset.Now);
    
    public static string GenerateBookingCode(DateOnly date)
    {
        var randomString = GenerateRandomString();
        return $"EE-{randomString}-{date:yyyyMMdd}";
    }

    public static string GenerateRandomString(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        Random random = new Random();
        char[] stringChars = new char[length];

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = chars[random.Next(chars.Length)];
        }

        return new string(stringChars);
    }
}