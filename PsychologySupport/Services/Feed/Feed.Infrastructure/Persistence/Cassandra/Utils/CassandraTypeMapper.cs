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
}
