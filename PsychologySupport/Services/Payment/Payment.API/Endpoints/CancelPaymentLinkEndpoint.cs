using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Payment.Application.Payments.Commands;

namespace Payment.API.Endpoints
{
    public record CancelPaymentLinkRequest(long PaymentCode, string? CancellationReason);
    public record CancelPaymentLinkResponse(
        PaymentLinkInformation PaymentInfo,
        string Message);

    public class CancelPaymentLinkEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/payments/payos/cancel", async (
                [FromBody] CancelPaymentLinkRequest request,
                ISender sender) =>
            {
                    var command = new CancelPaymentLinkCommand(
                        request.PaymentCode,
                        request.CancellationReason);

                    var result = await sender.Send(command);
                    var response = result.Adapt<CancelPaymentLinkResponse>();

                    return Results.Ok(response);
            })
            .WithName("CancelPaymentLink")
            .WithTags("PayOS Payments")
            .Produces<CancelPaymentLinkResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Cancels a specific payment link with optional cancellation reason")
            .WithSummary("Cancel Payment Link");
        }
    }
}
