namespace BuildingBlocks.Messaging.Events.Queries.Profile;

public record GetUserDataRequest(string? UserId, string? UserEmail = null);