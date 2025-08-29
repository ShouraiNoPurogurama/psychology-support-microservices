namespace ChatBox.API.Domains.AIChats.Dtos.AI;

public record GetAIChatHistoryResponseDto(
    Guid SessionId,
    List<AIMessageResponseDto> Messages
);