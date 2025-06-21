namespace ChatBox.API.Dtos;

public record AIMessageRequestDto(string UserMessage, Guid SessionId)
{
    public DateTime SentAt { get; init; } = DateTime.UtcNow;
}