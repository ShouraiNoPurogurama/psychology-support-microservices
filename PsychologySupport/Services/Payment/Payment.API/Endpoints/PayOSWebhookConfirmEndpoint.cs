using Carter;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;

namespace Payment.API.Endpoints;

public class PayOSWebhookConfirmEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/payos/confirm-webhook", async (
            [FromServices] PayOS payOS,
            [FromServices] IConfiguration configuration) =>
        {
                var webhookUrl = configuration["PayOS:WebhookUrl"];
                if (string.IsNullOrEmpty(webhookUrl))
                {
                    return Results.Problem(
                        detail: "Webhook URL is not configured",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                // Confirm webhook URL using PayOS SDK
                var result = await payOS.confirmWebhook(webhookUrl);
                return Results.Ok(new { WebhookUrl = result });
         })
        .WithName("ConfirmPayOSWebhook")
        .WithTags("PayOS Payments")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Confirms the PayOS webhook URL for receiving payment notifications")
        .WithSummary("Confirm PayOS Webhook URL");
    }
}