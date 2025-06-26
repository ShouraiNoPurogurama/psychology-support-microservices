using Carter;
using MediatR;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.Schedule.GetTotalSession
{
    public record GetTotalSessionResponse(List<TotalSessionDto> Sessions);

    public class GetTotalSessionEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/schedule/get-total-sessions", async ([AsParameters] GetTotalSessionQuery request, ISender sender) =>
            {
                var result = await sender.Send(request);
                var response = new GetTotalSessionResponse(result);
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Manager"))
            .WithName("GetTotalSessions")
            .WithTags("Schedules")
            .Produces<GetTotalSessionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get total session count within a date range")
            .WithSummary("Get Total Sessions");
        }
    }
}
