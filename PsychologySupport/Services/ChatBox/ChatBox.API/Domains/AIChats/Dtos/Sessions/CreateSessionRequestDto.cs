namespace ChatBox.API.Domains.AIChats.Dtos.Sessions;

public record CreateSessionRequestDto(Guid SessionId, string SessionName, DateTimeOffset CreatedDate);