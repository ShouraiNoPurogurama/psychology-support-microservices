using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.API.Endpoints;

public record CreateVnPayCallBackUrlForUpgradeSubscriptionRequest(UpgradeSubscriptionDto UpgradeSubscription);

public record CreateVnPayCallBackUrlForUpgradeSubscriptionResponse(string Url);

public class CreateVnPayCallBackUrlForUpgradeSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/payments/vnpay/subscription/upgrade",
                async ([FromBody] CreateVnPayCallBackUrlForUpgradeSubscriptionRequest request, ISender sender) =>
                {
                    var command = new CreateVnPayCallBackUrlForUpgradeSubscriptionCommand(request.UpgradeSubscription);

                    var result = await sender.Send(command);

                    var response = result.Adapt<CreateVnPayCallBackUrlForUpgradeSubscriptionResponse>();

                    return Results.Ok(response);
                })
            .WithName("CreateVnPayCallBackUrlForUpgradeSubscription")
            .WithTags("VnPay Payments")
            .Produces<CreateVnPayCallBackUrlForUpgradeSubscriptionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create VnPay CallBack Url For Upgrade Subscription")
            .WithSummary("Create VnPay CallBack Url For Upgrade Subscription");
    }
}