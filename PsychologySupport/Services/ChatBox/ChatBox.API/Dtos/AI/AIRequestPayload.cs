namespace ChatBox.API.Dtos.AI;

public record AIRequestPayload(
    string Context,
    string? Summarization,
    List<HistoryMessage> HistoryMessages
);