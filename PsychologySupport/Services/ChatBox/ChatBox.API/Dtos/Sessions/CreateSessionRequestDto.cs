namespace ChatBox.API.Dtos.Sessions;

public record CreateSessionRequestDto(Guid SessionId, string SessionName, DateTime CreatedDate);