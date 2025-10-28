namespace BuildingBlocks.Utils;

public class TimeUtils
{
    private static readonly TimeSpan UtcPlus7Offset = new TimeSpan(7, 0, 0);

    public static DateTimeOffset ConvertToUtcPlus7(DateTimeOffset dateTimeOffset)
        => dateTimeOffset.ToOffset(UtcPlus7Offset);

    public static DateTimeOffset ConvertToUtcPlus7WithNoChange(DateTimeOffset dateTimeOffset)
        => dateTimeOffset.ToOffset(UtcPlus7Offset).AddHours(-7);

    public static int GetAgeFromDate(DateTime dateOfBirth, DateTime? asOf = null)
    {
        var today = asOf?.Date ?? DateTime.UtcNow.Date;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--; // chưa tới sinh nhật năm nay
        return age;
    }

    public static int GetAgeFromDate(DateTimeOffset dateOfBirth, DateTimeOffset? asOf = null)
    {
        var today = asOf?.Date ?? DateTimeOffset.UtcNow.Date;
        var age = today.Year - dateOfBirth.Year;
        if (dateOfBirth.Date > today.AddYears(-age)) age--; // chưa tới sinh nhật năm nay
        return age;
    }
    
    public static int GetAgeFromDateTimeOffsetStr(string dateOfBirth, DateTimeOffset? asOf = null)
    {
        var cleaned = dateOfBirth.Trim('"');
        var birth = DateTimeOffset.Parse(cleaned, null, System.Globalization.DateTimeStyles.AssumeUniversal);
        var now = DateTimeOffset.UtcNow;
        var age = now.Year - birth.Year;
        if (now < birth.AddYears(age)) age--; // chưa tới sinh nhật năm nay
        return age;
    }
}