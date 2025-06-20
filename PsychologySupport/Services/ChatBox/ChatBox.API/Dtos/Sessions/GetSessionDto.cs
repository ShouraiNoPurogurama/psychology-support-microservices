namespace ChatBox.API.Dtos.Sessions;

public record GetSessionDto(Guid Id, string Name, DateTime CreatedDate);