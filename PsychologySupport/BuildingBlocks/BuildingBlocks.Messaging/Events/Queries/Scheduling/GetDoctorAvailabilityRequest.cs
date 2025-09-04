namespace BuildingBlocks.Messaging.Events.Queries.Scheduling
{
    public record GetDoctorAvailabilityRequest(List<Guid> DoctorIds, DateTime StartDate, DateTime EndDate);
}
