using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features02.UpgradeUserSubscription;

public record UpgradeUserSubscriptionV2Request(UpgradeUserSubscriptionDto UpgradeUserSubscriptionDto);

public record UpgradeUserSubscriptionV2Response(Guid Id, string PaymentUrl,long? PaymentCode);

public class UpgradeUserSubscriptionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v2/user-subscription/upgrade", async ([FromBody] UpgradeUserSubscriptionV2Request request, ISender sender, HttpContext httpContext) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.CanModifyPatientProfile(request.UpgradeUserSubscriptionDto.PatientId, httpContext.User))
                        throw new ForbiddenException();

                    var command = request.Adapt<UpgradeUserSubscriptionCommand>();

                    var result = await sender.Send(command);

                    var response = result.Adapt<UpgradeUserSubscriptionV2Response>();

                    return Results.Created($"/v2/userSubscriptions/{response.Id}/upgrade", response);
                }
            )
            .WithName("UpgradeUserSubscription v2")
            .WithTags("UserSubscriptions Version 2")
            .Produces<UpgradeUserSubscriptionV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Upgrade User Subscription")
            .WithSummary("Upgrade User Subscription");
    }
}