using BuildingBlocks.Dtos;

namespace BuildingBlocks.Messaging.Events.Auth;

public record GetOnlineUsersDataResponse(IEnumerable<UserDto> Users);