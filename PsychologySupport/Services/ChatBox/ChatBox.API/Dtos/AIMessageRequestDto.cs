namespace ChatBox.API.Dtos;

public record AIMessageRequestDto(List<AIMessageDto> History, string UserMessage, Guid SessionId); 