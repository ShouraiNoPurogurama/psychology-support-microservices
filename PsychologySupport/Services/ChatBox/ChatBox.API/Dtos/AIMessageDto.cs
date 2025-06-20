namespace ChatBox.API.Dtos;

public record AIMessageDto(bool SenderIsEmo, string Content, DateTime CreatedDate);