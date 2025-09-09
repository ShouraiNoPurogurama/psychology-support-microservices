namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Profile
{
    //TODO tí quay lại check
    public record PiiUpdatedIntegrationEvent(
        Guid SubjectRef,
        string FullName,
        string Email,
        string PhoneNumber
    ) : IntegrationEvent;
}
