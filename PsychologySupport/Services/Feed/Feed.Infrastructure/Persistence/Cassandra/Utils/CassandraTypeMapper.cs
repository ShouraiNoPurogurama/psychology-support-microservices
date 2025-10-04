using Cassandra;

namespace Feed.Infrastructure.Persistence.Cassandra.Utils;

internal static class CassandraTypeMapper
{
    public static LocalDate ToLocalDate(DateOnly dateOnly) 
        => new LocalDate(dateOnly.Year, dateOnly.Month, dateOnly.Day);
    
    public static DateOnly ToDateOnly(LocalDate localDate) 
        => new DateOnly(localDate.Year, localDate.Month, localDate.Day);
    
    public static TimeUuid ToTimeUuid(Guid guid)
        => TimeUuid.Parse(guid.ToString());
    
    public static Guid ToGuid(TimeUuid timeUuid)
        => Guid.Parse(timeUuid.ToString());
    
    /// <summary>
    /// Converts a .NET DateTimeOffset to a Cassandra TimeUuid.
    /// </summary>
    public static TimeUuid ToTimeUuid(DateTimeOffset timestamp)
    {
        return TimeUuid.NewId(timestamp);
    }

    /// <summary>
    /// Converts a Cassandra TimeUuid back to a .NET DateTimeOffset.
    /// </summary>
    public static DateTimeOffset ToDateTimeOffset(TimeUuid timeUuid)
    {
        return timeUuid.GetDate();
    }

}
