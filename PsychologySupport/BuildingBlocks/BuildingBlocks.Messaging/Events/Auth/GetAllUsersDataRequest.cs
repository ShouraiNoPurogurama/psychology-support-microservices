namespace BuildingBlocks.Messaging.Events.Auth;

public record GetAllUsersDataRequest(List<Guid>? UserIds = null, string? Role = null);