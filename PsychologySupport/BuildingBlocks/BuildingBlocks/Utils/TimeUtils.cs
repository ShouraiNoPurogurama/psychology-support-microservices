using System.Runtime.InteropServices;

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
    
    public static string Relative(DateTimeOffset when, DateTimeOffset now)
    {
        var d = now - when;
        if (d.TotalMinutes < 1) return "vừa xong";
        if (d.TotalHours   < 1) return $"{(int)d.TotalMinutes} phút trước";
        if (d.TotalDays    < 1) return $"{(int)d.TotalHours} giờ trước";
        if (d.TotalDays    < 7) return $"{(int)d.TotalDays} ngày trước";
        if (d.TotalDays   < 30) return $"{(int)(d.TotalDays / 7)} tuần trước";
        if (d.TotalDays  < 365) return $"{(int)(d.TotalDays / 30)} tháng trước";
        return $"{(int)(d.TotalDays / 365)} năm trước";
    }
    
    public static TimeZoneInfo Instance =>
        TimeZoneInfo.FindSystemTimeZoneById(
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? "SE Asia Standard Time"       // Windows
                : "Asia/Ho_Chi_Minh"            // Linux/macOS
        );
    
    
    public static DateOnly StartOfIsoWeek(DateOnly d)
    {
        var dow = (int)d.DayOfWeek; // Sun=0..Sat=6
        var iso = dow == 0 ? 7 : dow; // Sun -> 7
        return d.AddDays(-(iso - 1));
    }
}