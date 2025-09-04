using BuildingBlocks.Messaging.Events.Queries.Notification;
using MassTransit;
using Notification.API.Domains.Emails.ServiceContracts;

namespace Notification.API.Domains.Emails.EventHandlers;

public class HasSentEmailRecentlyRequestHandler(IEmailService emailService) : IConsumer<HasSentEmailRecentlyRequest>
{
    public async Task Consume(ConsumeContext<HasSentEmailRecentlyRequest> context)
    {
        var isRecentlySent = await emailService.HasSentEmailRecentlyAsync(context.Message.Email, context.CancellationToken);
        
        await context.RespondAsync(new HasSentEmailRecentlyResponse(isRecentlySent));
    }
}