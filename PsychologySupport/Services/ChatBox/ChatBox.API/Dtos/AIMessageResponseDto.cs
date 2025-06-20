namespace ChatBox.API.Dtos;

public record AIMessageResponseDto(
    Guid SessionId,
    bool SenderIsEmo,
    string Content,
    DateTime CreatedDate
);