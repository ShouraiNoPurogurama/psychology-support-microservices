using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Net.payOS;
using Net.payOS.Types;
using Payment.Application.Payments.Queries;
using System.Text;
using System.Text.Json;

namespace Payment.API.Endpoints;

public class PayOSWebhookProcessEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Endpoint cho webhook
        app.MapPost("/payments/payos/webhook", async (
        HttpRequest request,
        [FromServices] PayOS payOS,
        [FromServices] ISender sender) =>
        {
            try
            {
                request.EnableBuffering();
                using var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true);
                string rawBody = await reader.ReadToEndAsync();
                request.Body.Position = 0;

                Console.WriteLine($"[Webhook] Raw body: {rawBody}");

                var webhookBody = JsonSerializer.Deserialize<WebhookType>(rawBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (webhookBody == null)
                {
                    Console.WriteLine("[Webhook] Failed to deserialize body");
                    return Results.Problem("Invalid webhook data", statusCode: StatusCodes.Status400BadRequest);
                }

                Console.WriteLine($"[Webhook] Deserialized body: {JsonSerializer.Serialize(webhookBody)}");

                try
                {
                    WebhookData webhookData = payOS.verifyPaymentWebhookData(webhookBody);
                    Console.WriteLine($"[Webhook] Verified data: {JsonSerializer.Serialize(webhookData)}");

                    var command = new ProcessPayOSWebhookCommand(webhookData);
                    var result = await sender.Send(command);
                    Console.WriteLine($"[Webhook] Command processed: {JsonSerializer.Serialize(result)}");
                }
                catch (Exception ex)
                {
                    // Log the error but still return 200 OK to satisfy PayOS webhook registration
                    Console.WriteLine($"[Webhook Warning] Failed to process webhook data: {ex.Message}");
                }

                return Results.Ok(new { Message = "Webhook received successfully" });
            }
            catch (Exception ex)
            {
                // Log critical errors but return 200 OK to avoid webhook registration failure
                Console.WriteLine($"[Webhook Error] Critical failure: {ex.Message}");
                return Results.Ok(new { Message = "Webhook received but processing failed" });
            }
        })
        .WithName("ProcessPayOSWebhook")
        .WithTags("PayOS Payments")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Processes incoming PayOS webhook notifications for payment status updates")
        .WithSummary("Process PayOS Webhook");

        /// View Test

        //Endpoint cho ReturnUrl
        app.MapGet("/payment/callback", (string code, string id, string cancel, string status, string orderCode) =>
        {
            Console.WriteLine("Return URL data: " +
                $"Code={code ?? "N/A"}, ID={id ?? "N/A"}, Cancel={cancel ?? "N/A"}, " +
                $"Status={status ?? "N/A"}, OrderCode={orderCode ?? "N/A"}");

            return Results.Content($@"
                <h1>Return URL Received</h1>
                <p>Code: {code ?? "N/A"}</p>
                <p>ID: {id ?? "N/A"}</p>
                <p>Cancel: {cancel ?? "N/A"}</p>
                <p>Status: {status ?? "N/A"}</p>
                <p>Order Code: {orderCode ?? "N/A"}</p>
            ", "text/html");
        })
        .WithName("ReturnUrlCallback")
        .WithTags("PayOS Payments")
        .Produces(StatusCodes.Status200OK)
        .WithDescription("Handles redirect from PayOS after payment completion or cancellation")
        .WithSummary("Return URL Callback");

        // Endpoint cho CancelUrl
        app.MapGet("/payment/cancel", (string code, string id, string orderCode) =>
        {
            Console.WriteLine("Cancel URL data: " +
                $"Code={code ?? "N/A"}, ID={id ?? "N/A"}, OrderCode={orderCode ?? "N/A"}");

            return Results.Content($@"
                <h1>Cancel URL Received</h1>
                <p>Code: {code ?? "N/A"}</p>
                <p>ID: {id ?? "N/A"}</p>
                <p>Order Code: {orderCode ?? "N/A"}</p>
            ", "text/html");
        })
        .WithName("CancelUrlCallback")
        .WithTags("PayOS Payments")
        .Produces(StatusCodes.Status200OK)
        .WithDescription("Handles redirect from PayOS when payment is cancelled")
        .WithSummary("Cancel URL Callback");
    }
}