using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Common;
using Subscription.API.Data.Common;

namespace Subscription.API.UserSubscriptions.Features.v2.GetTotalSubscription
{
    public record GetTotalSubscriptionV2Request(
        DateOnly StartDate,
        DateOnly EndDate,
        Guid? PatientId,
        SubscriptionStatus? Status
    );

    public record GetTotalSubscriptionV2Response(long TotalCount);
    public class GetTotalSubscriptionV2Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v2/user-subscriptions/total", async (
                    [FromBody] GetTotalSubscriptionV2Request request,
                    ISender sender,
                    HttpContext httpContext) =>
            {
                // Kiểm tra quyền
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User) &&
                    !AuthorizationHelpers.IsExclusiveAccess(httpContext.User))
                    throw new ForbiddenException();

                var query = request.Adapt<GetTotalSubscriptionQuery>();

                var result = await sender.Send(query);

                var response = result.Adapt<GetTotalSubscriptionV2Response>();

                return Results.Ok(response);
            })
                .WithName("GetTotalUserSubscriptionsV2")
                .WithTags("Dashboard Version 2")
                .Produces<GetTotalSubscriptionV2Response>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Get Total User Subscriptions (Version 2)")
                .WithSummary("Get Total User Subscriptions");
        }
    }
}
