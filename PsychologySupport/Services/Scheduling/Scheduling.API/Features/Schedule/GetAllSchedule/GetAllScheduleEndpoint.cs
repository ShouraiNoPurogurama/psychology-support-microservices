using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features.Schedule.GetAllSchedule
{
    public record GetAllSchedulesRequest(
     int PageIndex,
     int PageSize,
     string? Search,
     string? SortBy,
     string? SortOrder,
     Guid? DoctorId,
     Guid? PatientId);
    public record GetAllSchedulesResponse(PaginatedResult<ScheduleDto> Schedules);
    public class GetAllScheduleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/schedules", async ([AsParameters] GetAllSchedulesRequest request, ISender sender) =>
            {
                var query = request.Adapt<GetAllSchedulesQuery>();
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllSchedulesResponse>();
                return Results.Ok(response);
            })
            .WithName("GetAllSchedules")
            .WithTags("Schedules")
            .Produces<GetAllSchedulesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All Schedules")
            .WithSummary("Get All Schedules");
        }
    }

}
