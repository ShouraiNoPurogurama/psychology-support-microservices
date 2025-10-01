namespace ChatBox.API.Domains.AIChats.Dtos.AI;

public record AIMessageDto(bool SenderIsEmo, string Content, DateTimeOffset CreatedDate);