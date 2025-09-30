using BuildingBlocks.Messaging.Events.Queries.Notification;
using MassTransit;
using Notification.API.Features.Emails.Contracts;

namespace Notification.API.Features.Emails.SendEmail;

public class HasSentEmailRecentlyRequestHandler(IEmailService emailService) : IConsumer<HasSentEmailRecentlyRequest>
{
    public async Task Consume(ConsumeContext<HasSentEmailRecentlyRequest> context)
    {
        var isRecentlySent = await emailService.HasSentEmailRecentlyAsync(context.Message.Email, context.CancellationToken);
        
        await context.RespondAsync(new HasSentEmailRecentlyResponse(isRecentlySent));
    }
}