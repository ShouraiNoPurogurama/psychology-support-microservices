namespace BuildingBlocks.Messaging.Events.Queries.Scheduling
{
    public record GetDoctorAvailabilityResponse(Dictionary<Guid, bool> AvailabilityMap);
}
