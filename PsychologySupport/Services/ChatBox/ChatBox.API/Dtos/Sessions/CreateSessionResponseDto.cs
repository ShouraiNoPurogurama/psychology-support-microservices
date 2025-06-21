namespace ChatBox.API.Dtos.Sessions;

public record CreateSessionResponseDto(Guid Id, string Name, AIMessageResponseDto InitialMessage);