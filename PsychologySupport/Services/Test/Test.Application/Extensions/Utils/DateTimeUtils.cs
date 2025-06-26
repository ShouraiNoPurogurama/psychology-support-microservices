namespace Test.Application.Extensions.Utils;

public static class DateTimeUtils
{
    public static string FormatCompletionTime(TimeSpan span)
    {
        if (span.TotalMinutes < 1)
            return $"{span.Seconds} giây";
        if (span.Seconds == 0)
            return $"{(int)span.TotalMinutes} phút";
        return $"{(int)span.TotalMinutes} phút {span.Seconds} giây";
    }
}