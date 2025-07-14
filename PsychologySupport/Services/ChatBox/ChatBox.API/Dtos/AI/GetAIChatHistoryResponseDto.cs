namespace ChatBox.API.Dtos.AI;

public record GetAIChatHistoryResponseDto(
    Guid SessionId,
    List<AIMessageResponseDto> Messages
);