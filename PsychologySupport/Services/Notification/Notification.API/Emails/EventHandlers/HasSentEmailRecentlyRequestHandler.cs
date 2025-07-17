using BuildingBlocks.Messaging.Events.Notification;
using MassTransit;
using Notification.API.Emails.ServiceContracts;

namespace Notification.API.Emails.EventHandlers;

public class HasSentEmailRecentlyRequestHandler(IEmailService emailService) : IConsumer<HasSentEmailRecentlyRequest>
{
    public async Task Consume(ConsumeContext<HasSentEmailRecentlyRequest> context)
    {
        var isRecentlySent = await emailService.HasSentEmailRecentlyAsync(context.Message.Email, context.CancellationToken);
        
        await context.RespondAsync(new HasSentEmailRecentlyResponse(isRecentlySent));
    }
}