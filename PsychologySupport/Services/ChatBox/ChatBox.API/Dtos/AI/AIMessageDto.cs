namespace ChatBox.API.Dtos.AI;

public record AIMessageDto(bool SenderIsEmo, string Content, DateTime CreatedDate);