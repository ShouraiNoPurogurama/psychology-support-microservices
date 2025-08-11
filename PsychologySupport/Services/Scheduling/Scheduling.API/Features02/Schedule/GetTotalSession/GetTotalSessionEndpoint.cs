using Carter;
using MediatR;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features02.Schedule.GetTotalSession
{
    public record GetTotalSessionV2Response(List<TotalSessionDto> Sessions);

    public class GetTotalSessionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v2/me/schedule/totalSessions", async ([AsParameters] GetTotalSessionQuery request, ISender sender) =>
            {
                var result = await sender.Send(request);
                var response = new GetTotalSessionV2Response(result);
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Manager"))
            .WithName("GetTotalSessions v2")
            .WithTags("Schedules Version 2")
            .Produces<GetTotalSessionV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get total session count within a date range")
            .WithSummary("Get Total Sessions");
        }
    }
}
