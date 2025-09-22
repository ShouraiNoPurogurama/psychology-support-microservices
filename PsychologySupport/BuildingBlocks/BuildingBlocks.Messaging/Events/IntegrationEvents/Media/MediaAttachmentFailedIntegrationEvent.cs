namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Media;

public record MediaAttachmentFailedIntegrationEvent(Guid OwnerId, string OwnerType, Dictionary<Guid, string> FailedMedia, string Reason) : IntegrationEvent;