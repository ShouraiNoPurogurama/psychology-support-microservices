using ChatBox.API.Dtos.Gemini;
using ChatBox.API.Events;
using ChatBox.API.Services;
using MediatR;

namespace ChatBox.API.EventHandlers;

public class AIChatSessionSummarizedEventHandler(
    ILogger<AIChatSessionSummarizedEvent> logger,
    SummarizationService summarizationService
)
    : INotificationHandler<AIChatSessionSummarizedEvent>
{
    public async Task Handle(AIChatSessionSummarizedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("*** Handling AIChatSessionSummarizedEvent for Session ID: {SessionId}",
            notification.SessionId);

        var newMessages = notification.AIMessages;
        var sessionId = notification.SessionId;
        var userId = notification.UserId;


        var contentDtos = newMessages.Select(m => new GeminiContentDto(
                m.SenderIsEmo ? "model" : "user",
                [new GeminiContentPartDto(m.Content)]
            ))
            .ToList();

        var summary = await summarizationService.CallGeminiSummarizationV1BetaAsync(contentDtos);

        var result = await summarizationService.UpdateSessionSummarizationAsync(userId, sessionId, summary, newMessages.Count);

        switch (result)
        {
            case true:
                logger.LogInformation("Session {SessionId} summarized successfully.", sessionId);
                break;
            case false:
                logger.LogWarning("Failed to summarize session {SessionId}.", sessionId);
                break;
        }
    }
}