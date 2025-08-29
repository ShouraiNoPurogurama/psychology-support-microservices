namespace ChatBox.API.Domains.AIChats.Dtos.AI;

public record AIMessageResponseDto(
    Guid SessionId,
    bool SenderIsEmo,
    string Content,
    DateTime CreatedDate
);