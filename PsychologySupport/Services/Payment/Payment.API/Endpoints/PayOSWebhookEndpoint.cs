using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Commands;


namespace Payment.API.Endpoints;

public class PayOSWebhookEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/payos/webhook", async (
            HttpContext context,
            [FromServices] IPayOSService payOSService,
            [FromServices] ISender sender) =>
        {
            using var reader = new StreamReader(context.Request.Body);
            var webhookJson = await reader.ReadToEndAsync();

            try
            {
                var webhookData = await payOSService.VerifyWebhookDataAsync(webhookJson);

                var command = new ProcessPayOSWebhookCommand(webhookData);
                var result = await sender.Send(command);

                return Results.Ok(new { Message = "Webhook processed successfully" });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Failed to process webhook: {ex.Message}",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
        })
        .WithName("ProcessPayOSWebhook")
        .WithTags("PayOS Payments")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Process PayOS Webhook")
        .WithSummary("Process PayOS Webhook");

        app.MapPost("/payments/payos/confirm-webhook", async (
            [FromServices] IPayOSService payOSService,
            [FromServices] IConfiguration configuration) =>
        {
            try
            {
                var webhookUrl = configuration["PayOS:WebhookUrl"];
                if (string.IsNullOrEmpty(webhookUrl))
                {
                    return Results.Problem(
                        detail: "Webhook URL is not configured",
                        statusCode: StatusCodes.Status400BadRequest
                    );
                }

                var result = await payOSService.ConfirmWebhookAsync(webhookUrl);
                return Results.Ok(new { WebhookUrl = result });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: $"Failed to confirm webhook: {ex.Message}",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }
        })
        .WithName("ConfirmPayOSWebhook")
        .WithTags("PayOS Payments")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Confirm PayOS Webhook URL")
        .WithSummary("Confirm PayOS Webhook URL");
    }
}