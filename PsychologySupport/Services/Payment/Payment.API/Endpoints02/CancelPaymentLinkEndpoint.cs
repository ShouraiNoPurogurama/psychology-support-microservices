using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using Payment.Application.Payments.Commands;

namespace Payment.API.Endpoints02
{
    public record CancelPaymentLinkV2Request(long PaymentCode, string? CancellationReason);
    public record CancelPaymentLinkV2Response(
        PaymentLinkInformation PaymentInfo,
        string Message);

    public class CancelPaymentLinkV2Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/v2/payments/payos/link-information/cancel", async (
                [FromBody] CancelPaymentLinkV2Request request,
                ISender sender) =>
            {
                    var command = new CancelPaymentLinkCommand(
                        request.PaymentCode,
                        request.CancellationReason);

                    var result = await sender.Send(command);
                    var response = result.Adapt<CancelPaymentLinkV2Response>();

                    return Results.Ok(response);
            })
            .WithName("CancelPaymentLink v2")
            .WithTags("PayOS Payments Version 2")
            .Produces<CancelPaymentLinkV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Cancels a specific payment link with optional cancellation reason")
            .WithSummary("Cancel Payment Link");
        }
    }
}
