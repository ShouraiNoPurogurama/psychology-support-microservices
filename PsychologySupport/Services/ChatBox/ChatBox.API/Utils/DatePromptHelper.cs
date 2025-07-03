using System.Text;

namespace ChatBox.API.Utils;

public static class DatePromptHelper
{
    //Cache patterns để tăng performance
    private static readonly HashSet<string> TimePatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "hôm nay", "nay", "today", "giờ là", "bây giờ", "mấy giờ", "now", "current", "lúc này", "thời điểm này",
        "hiện tại", "present", "right now", "at the moment"
    };

    private static readonly HashSet<string> DatePatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "ngày mấy", "ngày bao nhiêu", "ngày gì", "thứ mấy", "mấy giờ", "giờ mấy",
        "what day", "what time", "what date", "date", "day is it", "time is it",
        "tháng mấy", "năm mấy", "năm nay", "month", "year",
        "mùng mấy", "số mấy", "which day", "which date", "what's the date",
        "what's the day", "what's the time", "today is", "today's date"
    };

    // Các pattern loại trừ (không phải hỏi về hiện tại)
    private static readonly HashSet<string> ExclusionPatterns = new(StringComparer.OrdinalIgnoreCase)
    {
        "mai", "tomorrow", "hôm qua", "yesterday", "tuần sau", "next week",
        "tháng sau", "next month", "năm sau", "next year", "hôm kia",
        "ngày kia", "tuần trước", "last week", "tháng trước", "last month"
    };

    // Cache dayOfWeek mapping
    private static readonly Dictionary<DayOfWeek, string> VietnameseDayOfWeek = new()
    {
        { DayOfWeek.Monday, "thứ Hai" },
        { DayOfWeek.Tuesday, "thứ Ba" },
        { DayOfWeek.Wednesday, "thứ Tư" },
        { DayOfWeek.Thursday, "thứ Năm" },
        { DayOfWeek.Friday, "thứ Sáu" },
        { DayOfWeek.Saturday, "thứ Bảy" },
        { DayOfWeek.Sunday, "Chủ Nhật" }
    };

    public static bool IsDateQuestion(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        var lowerInput = input.ToLowerInvariant();

        //Loại bỏ các câu hỏi không phải về hiện tại
        if (ExclusionPatterns.Any(pattern => lowerInput.Contains(pattern)))
            return false;

        //Kiểm tra có chứa pattern thời gian và pattern ngày tháng
        bool hasTimePattern = TimePatterns.Any(pattern => lowerInput.Contains(pattern));
        bool hasDatePattern = DatePatterns.Any(pattern => lowerInput.Contains(pattern));

        return hasTimePattern && hasDatePattern;
    }

    public static string PrependDateTimePrompt(string input, string? userTimezone = null)
    {
        if (!IsDateQuestion(input))
            return input;

        var now = GetCurrentTimeByTimezone(userTimezone);
        var promptBuilder = new StringBuilder();

        var lowerInput = input.ToLowerInvariant();
        
        //Thêm thông tin ngày tháng
        if (ContainsDateInfo(lowerInput))
        {
            promptBuilder.Append($"Hôm nay là ngày {now.Day} tháng {now.Month} năm {now.Year}, ");
            promptBuilder.Append($"{GetVietnameseDayOfWeek(now.DayOfWeek)}. ");
        }

        //Thêm thông tin giờ
        if (ContainsTimeInfo(lowerInput))
        {
            promptBuilder.Append($"Bây giờ là {now:HH:mm}. ");
        }

        //Thêm thông tin tháng/năm nếu cần
        if (ContainsMonthYearInfo(lowerInput))
        {
            promptBuilder.Append($"Tháng này là tháng {now.Month}, năm nay là {now.Year}. ");
        }

        //Thêm hướng dẫn ngắn gọn
        promptBuilder.Append("Trả lời dựa trên thông tin thời gian trên. ");

        return promptBuilder + input;
    }

    private static bool ContainsDateInfo(string input)
    {
        string[] dateKeywords = { "ngày", "thứ", "day", "date" };
        return dateKeywords.Any(input.Contains);
    }

    private static bool ContainsTimeInfo(string input)
    {
        string[] timeKeywords = { "giờ", "time", "mấy giờ", "what time" };
        return timeKeywords.Any(input.Contains);
    }

    private static bool ContainsMonthYearInfo(string input)
    {
        string[] monthYearKeywords = { "tháng", "năm", "month", "year" };
        return monthYearKeywords.Any(input.Contains);
    }

    private static string GetVietnameseDayOfWeek(DayOfWeek dayOfWeek)
    {
        return VietnameseDayOfWeek.TryGetValue(dayOfWeek, out var result) ? result : "";
    }
    
    // Overload method với timezone offset
    public static string PrependDateTimePrompt(string input, int timezoneOffsetHours)
    {
        var timezone = TimeZoneInfo.CreateCustomTimeZone(
            $"UTC{(timezoneOffsetHours >= 0 ? "+" : "")}{timezoneOffsetHours}",
            TimeSpan.FromHours(timezoneOffsetHours),
            $"UTC{(timezoneOffsetHours >= 0 ? "+" : "")}{timezoneOffsetHours}",
            $"UTC{(timezoneOffsetHours >= 0 ? "+" : "")}{timezoneOffsetHours}"
        );

        return PrependDateTimePrompt(input, timezone.Id);
    }

    private static DateTime GetCurrentTimeByTimezone(string? userTimezone)
    {
        if (string.IsNullOrEmpty(userTimezone))
            return DateTime.Now;

        try
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById(userTimezone);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
        }
        catch (TimeZoneNotFoundException)
        {
            if (TryParseTimezoneOffset(userTimezone, out int offsetHours))
            {
                return DateTime.UtcNow.AddHours(offsetHours);
            }

            return DateTime.Now;
        }
    }
    
    private static bool TryParseTimezoneOffset(string timezone, out int offsetHours)
    {
        offsetHours = 0;
        
        if (string.IsNullOrEmpty(timezone))
            return false;

        var offset = timezone.Replace("UTC", "", StringComparison.OrdinalIgnoreCase).Trim();
        
        if (double.TryParse(offset, out double hours))
        {
            offsetHours = (int)hours;
            return true;
        }

        return false;
    }

}