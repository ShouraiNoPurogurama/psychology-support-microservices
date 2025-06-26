using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Scheduling.API.Common;
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
            app.MapGet("/schedules", async ([AsParameters] GetAllSchedulesRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (request.PatientId is Guid patientId)
                {
                    if (!AuthorizationHelpers.CanViewPatientProfile(patientId, httpContext.User))
                        return Results.Forbid();
                }

                var query = request.Adapt<GetAllSchedulesQuery>();
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllSchedulesResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllSchedules")
            .WithTags("Schedules")
            .Produces<GetAllSchedulesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All Schedules")
            .WithSummary("Get All Schedules");
        }
    }

}
