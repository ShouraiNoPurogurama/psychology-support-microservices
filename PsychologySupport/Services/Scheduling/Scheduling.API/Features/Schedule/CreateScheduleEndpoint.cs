using Carter;
using MediatR;
using Scheduling.API.Features.Schedule.CreateSchedule;

namespace Scheduling.API.Features.Schedule;

public record CreateScheduleRequest(
    Guid PatientId,
    Guid? DoctorId);

public record CreateScheduleResponse(
    Guid? ScheduleId);

public class CreateScheduleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/schedules/dummy", async (CreateScheduleRequest request, ISender sender) =>
        {
            var command = new CreateScheduleCommand(request.PatientId, request.DoctorId);
            var result = await sender.Send(command);
            return Results.Ok(new CreateScheduleResponse(result.ScheduleId));
        })
        .WithName("CreateSchedule")
        .WithTags("Schedules")
        .Produces<CreateScheduleResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create a new schedule")
        .WithSummary("Create a new schedule");
    }
}