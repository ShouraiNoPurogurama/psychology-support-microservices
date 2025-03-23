namespace BuildingBlocks.Messaging.Events.Auth;

public record GetUserDataRequest(string? UserId, string? UserEmail = null);