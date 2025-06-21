namespace ChatBox.API.Dtos;

public record AIMessageRequestDto(List<AIMessageDto> History, string UserMessage, Guid SessionId)
{
    public DateTime SentAt { get; init; } = DateTime.UtcNow;
}