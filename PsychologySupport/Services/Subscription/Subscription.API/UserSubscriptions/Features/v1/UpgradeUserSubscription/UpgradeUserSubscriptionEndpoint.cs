using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.v1.UpgradeUserSubscription;

public record UpgradeUserSubscriptionRequest(UpgradeUserSubscriptionDto UpgradeUserSubscriptionDto);

public record UpgradeUserSubscriptionResponse(Guid Id, string PaymentUrl,long? PaymentCode);

public class UpgradeUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/user-subscriptions/upgrade", async ([FromBody] UpgradeUserSubscriptionRequest request, ISender sender, HttpContext httpContext) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.CanModifyPatientProfile(request.UpgradeUserSubscriptionDto.PatientId, httpContext.User))
                        throw new ForbiddenException();

                    var command = request.Adapt<UpgradeUserSubscriptionCommand>();

                    var result = await sender.Send(command);

                    var response = result.Adapt<UpgradeUserSubscriptionResponse>();

                    return Results.Created($"/user-subscriptions/{response.Id}", response);
                }
            )
            .WithName("UpgradeUserSubscription")
            .WithTags("UserSubscriptions")
            .Produces<UpgradeUserSubscriptionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Upgrade User Subscription")
            .WithSummary("Upgrade User Subscription");
    }
}