namespace ChatBox.API.Domains.AIChats.Dtos.AI;

public record AIMessageRequestDto(string UserMessage, Guid SessionId)
{
    public DateTime SentAt { get; init; } = DateTime.UtcNow;
}