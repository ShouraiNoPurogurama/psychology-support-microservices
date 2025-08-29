namespace ChatBox.API.Domains.AIChats.Dtos.Sessions;

public record GetSessionDto(Guid Id, string Name, DateTime CreatedDate);