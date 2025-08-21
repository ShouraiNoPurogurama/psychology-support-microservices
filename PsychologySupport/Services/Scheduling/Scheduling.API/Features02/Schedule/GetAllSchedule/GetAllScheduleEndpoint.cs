using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Scheduling.API.Common;
using Scheduling.API.Dtos;

namespace Scheduling.API.Features02.Schedule.GetAllSchedule
{
    public record GetAllSchedulesV2Request(
     int PageIndex,
     int PageSize,
     string? Search,
     string? SortBy,
     string? SortOrder,
     Guid? DoctorId,
     Guid? PatientId);
    public record GetAllSchedulesV2Response(PaginatedResult<ScheduleDto> Schedules);
    public class GetAllScheduleEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v2/me/schedules", async ([AsParameters] GetAllSchedulesV2Request request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (request.PatientId is Guid patientId)
                {
                    if (!AuthorizationHelpers.CanViewPatientProfile(patientId, httpContext.User))
                        return Results.Problem(
                              statusCode: StatusCodes.Status403Forbidden,
                              title: "Forbidden",
                              detail: "You do not have permission to access this resource."
                          );
                }

                var query = request.Adapt<GetAllSchedulesQuery>();
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllSchedulesV2Response>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllSchedules v2")
            .WithTags("Schedules Version 2")
            .Produces<GetAllSchedulesV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All Schedules")
            .WithSummary("Get All Schedules");
        }
    }

}
