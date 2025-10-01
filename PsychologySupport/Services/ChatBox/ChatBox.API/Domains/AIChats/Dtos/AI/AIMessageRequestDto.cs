namespace ChatBox.API.Domains.AIChats.Dtos.AI;

public record AIMessageRequestDto(string UserMessage, Guid SessionId)
{
    public DateTimeOffset SentAt { get; init; } = DateTimeOffset.UtcNow;
}