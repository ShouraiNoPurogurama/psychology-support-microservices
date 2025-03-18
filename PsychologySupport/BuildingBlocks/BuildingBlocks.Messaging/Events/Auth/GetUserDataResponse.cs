namespace BuildingBlocks.Messaging.Events.Auth;

public record GetUserDataResponse(Guid Id, string UserName, string FullName);