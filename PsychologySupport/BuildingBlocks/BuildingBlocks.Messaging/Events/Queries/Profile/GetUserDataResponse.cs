namespace BuildingBlocks.Messaging.Events.Queries.Profile;

public record GetUserDataResponse(Guid Id, string UserName, string FullName, IEnumerable<string> FCMTokens);