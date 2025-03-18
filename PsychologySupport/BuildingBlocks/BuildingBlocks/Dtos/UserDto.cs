namespace BuildingBlocks.Dtos;

public record UserDto(Guid Id, string UserName, string FullName, bool IsOnline);