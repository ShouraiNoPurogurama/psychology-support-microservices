namespace BuildingBlocks.Messaging.Events.Queries.Profile;

public record GetAllUsersDataRequest(List<Guid>? UserIds = null, string? Role = null);