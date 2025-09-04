namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Profile
{
    public record DoctorProfileCreatedResponseEvent(Guid UserId, bool Success);
}
