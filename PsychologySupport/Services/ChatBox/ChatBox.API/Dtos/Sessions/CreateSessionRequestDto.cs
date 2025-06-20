namespace ChatBox.API.Dtos.Sessions;

public record CreateSessionResponseDto(Guid SessionId, string SessionName, DateTime CreatedDate);