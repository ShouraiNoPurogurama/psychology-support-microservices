using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Commands;
using Payment.Application.Payments.Dtos;

namespace Payment.API.Endpoints;

public record CreatePayOSCallBackUrlForUpgradeSubscriptionRequest(UpgradeSubscriptionDto UpgradeSubscription);
public record CreatePayOSCallBackUrlForUpgradeSubscriptionResponse(string Url);

public class CreatePayOSCallBackUrlForUpgradeSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/payments/payos/upgrade-subscription", async (
            [FromBody] CreatePayOSCallBackUrlForUpgradeSubscriptionRequest request,
            ISender sender) =>
        {
            var command = new CreatePayOSCallBackUrlForUpgradeSubscriptionCommand(request.UpgradeSubscription);
            var result = await sender.Send(command);
            var response = result.Adapt<CreatePayOSCallBackUrlForUpgradeSubscriptionResponse>();
            return Results.Ok(response);
        })
        .WithName("CreatePayOSCallBackUrlForUpgradeSubscription")
        .WithTags("PayOS Payments")
        .Produces<CreatePayOSCallBackUrlForUpgradeSubscriptionResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create PayOS CallBack Url For Upgrade Subscription")
        .WithSummary("Create PayOS CallBack Url For Upgrade Subscription");
    }
}
