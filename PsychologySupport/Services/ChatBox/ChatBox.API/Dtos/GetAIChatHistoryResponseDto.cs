namespace ChatBox.API.Dtos;

public record GetAIChatHistoryResponseDto(
    Guid SessionId,
    List<AIMessageResponseDto> Messages
);