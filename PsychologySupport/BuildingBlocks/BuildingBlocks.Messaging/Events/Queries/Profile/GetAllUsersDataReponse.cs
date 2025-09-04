using BuildingBlocks.Dtos;

namespace BuildingBlocks.Messaging.Events.Queries.Profile;

public record GetOnlineUsersDataResponse(IEnumerable<UserDto> Users);