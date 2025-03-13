using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.Schedule.GetAllSchedule
{
    public record GetAllSchedulesResponse(PaginatedResult<ScheduleDto> Schedules);
    public class GetAllScheduleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/schedules", async ([AsParameters] GetAllSchedulesQuery request, ISender sender) =>
            {
                var result = await sender.Send(request);
                var response = result.Adapt<GetAllSchedulesResponse>();
                return Results.Ok(response);
            })
            .WithName("GetAllSchedules")
            .Produces<GetAllSchedulesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All Schedules")
            .WithSummary("Get All Schedules");
        }
    }

}
