using BuildingBlocks.CQRS;
using Carter;
using MediatR;
using Scheduling.API.Dtos;
using Scheduling.API.Features.Schedule.GetTotalSession;

namespace Scheduling.API.Features02.Schedule.GetTotalActivities
{
    public record GetTotalActivitiesV2Response(TotalActivityDto Activities);

    public class GetTotalActivitiesV2Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v2/me/schedule/activities/total", async ([AsParameters] GetTotalActivitiesQuery request, ISender sender) =>
            {
                var result = await sender.Send(request);
                var response = new GetTotalActivitiesV2Response(result);
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetTotalActivities v2")
            .WithTags("Schedules Version 2")
            .Produces<GetTotalActivitiesV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get total activity times within a date range")
            .WithSummary("Get Total Activities");
        }
    }


}
