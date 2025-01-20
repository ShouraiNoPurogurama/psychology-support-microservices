namespace BuildingBlocks.Utils;

public class TimeUtils
{
    private static readonly TimeSpan UtcPlus7Offset = new TimeSpan(7, 0, 0);

    public static DateTimeOffset ConvertToUtcPlus7(DateTimeOffset dateTimeOffset)
        => dateTimeOffset.ToOffset(UtcPlus7Offset);

    public static DateTimeOffset ConvertToUtcPlus7WithNoChange(DateTimeOffset dateTimeOffset)
        => dateTimeOffset.ToOffset(UtcPlus7Offset).AddHours(-7);
}