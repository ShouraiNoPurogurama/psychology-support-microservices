namespace BuildingBlocks.Messaging.Events.IntegrationEvents.ImageStorage
{
    public record ImageUploadedIntegrationEvent(
        Guid RequestId, 
        string Name,
        byte[] Data,
        string Extension,
        string OwnerType, 
        Guid OwnerId  
    );

}
