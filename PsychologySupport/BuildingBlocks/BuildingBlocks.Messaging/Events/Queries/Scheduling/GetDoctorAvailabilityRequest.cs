namespace BuildingBlocks.Messaging.Events.Queries.Scheduling
{
    public record GetDoctorAvailabilityRequest(List<Guid> DoctorIds, DateTimeOffset StartDate, DateTimeOffset EndDate);
}
