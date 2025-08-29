using Carter;
using Notification.API.Domains.Emails.Events;

namespace Notification.API.Domains.Emails.Features;

public record SendEmailRequest(Guid EventId, string To, string Subject, string Body);

public class SendEmailEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/emails/send", async (SendEmailRequest request, IMediator mediator) =>
            {
                var notification = new SendEmailEvent(request.EventId, request.To, request.Subject, request.Body);
                await mediator.Publish(notification);

                return Results.Ok();
            })
            .WithTags("Email");
    }
}