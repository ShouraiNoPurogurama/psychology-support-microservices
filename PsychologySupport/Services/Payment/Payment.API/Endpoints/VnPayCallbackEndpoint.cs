using Carter;
using MediatR;
using Payment.Application.Payments.Dtos;
using Payment.Application.Payments.Queries;

namespace Payment.API.Endpoints;

public class VnPayCallbackEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("payments/vnpay/callback", async ([AsParameters] VnPayCallbackDto vnPayCallbackRequest, ISender sender) =>
        {
            var query = new VnPayCallbackQuery(vnPayCallbackRequest);

            var result = await sender.Send(query);

            return Results.Ok(result);
        })
        .WithName("VnPayCallback")
            .WithTags("VnPay Payments")
            .Produces<VnPayCallbackResult>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("VnPay Callback Endpoint")
            .WithSummary("Handle VnPay Callback");
    }
}