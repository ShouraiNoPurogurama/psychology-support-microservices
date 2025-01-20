namespace BuildingBlocks.Utils;

public class CoreUtils
{
    public static DateTimeOffset SystemTimeNow => TimeUtils.ConvertToUtcPlus7(DateTimeOffset.Now);
}