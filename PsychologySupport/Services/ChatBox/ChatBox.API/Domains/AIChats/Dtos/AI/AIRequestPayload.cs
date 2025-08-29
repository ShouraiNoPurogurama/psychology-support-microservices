namespace ChatBox.API.Domains.AIChats.Dtos.AI;

public record AIRequestPayload(
    string Context,
    string? Summarization,
    List<HistoryMessage> HistoryMessages
);