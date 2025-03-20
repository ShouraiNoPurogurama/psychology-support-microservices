using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Subscription.API.Data.Common;

namespace Subscription.API.UserSubscriptions.Features.GetTotalSubscription
{
        public class GetTotalSubscriptionEndpoint : ICarterModule
        {
            public void AddRoutes(IEndpointRouteBuilder app)
            {
                app.MapGet("/user-subscriptions/total", async (
                    [FromQuery] DateOnly startDate,
                    [FromQuery] DateOnly endDate,
                    [FromQuery] Guid? patientId,
                    [FromQuery] SubscriptionStatus? status,
                    ISender sender) =>
                {
                    var query = new GetTotalSubscriptionQuery(startDate, endDate, patientId, status);
                    var result = await sender.Send(query);

                    return Results.Ok(result);
                })
                .WithName("GetTotalUserSubscriptions")
                .Produces<GetTotalSubscriptionResult>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Get Total User Subscriptions")
                .WithSummary("Get Total User Subscriptions");
            }
        }

}
