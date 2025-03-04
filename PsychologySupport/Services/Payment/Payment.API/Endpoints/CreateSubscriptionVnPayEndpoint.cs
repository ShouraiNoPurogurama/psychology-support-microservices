using Carter;
using Mapster;
using MediatR;
using Payment.Application.Payment.Commands;
using Payment.Application.Payment.Dtos;

namespace Payment.API.Endpoints;

public record CreateSubscriptionVnPayRequest(BuySubscriptionDto BuySubscription);

public record CreateSubscriptionVnPayResponse(string Url);

public class CreateSubscriptionVnPayEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payment/vnpay/subscription", async (CreateSubscriptionVnPayRequest request, ISender sender) =>
            {
                var command = new CreateSubscriptionVnPayCommand(request.BuySubscription);
                
                var result = await sender.Send(command);

                var response = result.Adapt<CreateSubscriptionVnPayResponse>();
                
                return Results.Ok(response);
            })
            .WithName("CreateSubscriptionVnPay")
            .Produces<CreateSubscriptionVnPayResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create a new subscription using VNPay")
            .WithSummary("Create Subscription VnPay");
    }
}