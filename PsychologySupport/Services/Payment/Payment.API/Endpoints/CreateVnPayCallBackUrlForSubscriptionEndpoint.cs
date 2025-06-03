using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.API.Endpoints;

public record CreateVnPayCallBackUrlForSubscriptionRequest(BuySubscriptionDto BuySubscription);

public record CreateVnPayCallBackUrlForSubscriptionResponse(string Url);

public class CreateVnPayCallBackUrlForSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/vnpay/subscription", async ([FromBody]CreateVnPayCallBackUrlForSubscriptionRequest request, ISender sender) =>
            {
                var command = new CreateVnPayCallBackUrlForSubscriptionCommand(request.BuySubscription);
                
                var result = await sender.Send(command);

                var response = result.Adapt<CreateVnPayCallBackUrlForSubscriptionResponse>();
                
                return Results.Ok(response);
            })
            .WithName("CreateVnPayCallBackUrlForSubscription")
            .WithTags("Subscription Payments")
            .Produces<CreateVnPayCallBackUrlForSubscriptionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create VnPay CallBack Url For Subscription")
            .WithSummary("Create VnPay CallBack Url For Subscription");
    }
}