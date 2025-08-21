using Carter;
using MediatR;
using Scheduling.API.Features.Schedule.CreateSchedule;

namespace Scheduling.API.Features02.Schedule.CreateSchedule;

public record CreateScheduleV2Request(
    Guid PatientId,
    Guid? DoctorId);

public record CreateScheduleV2Response(
    Guid? ScheduleId);

public class CreateScheduleV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v2/schedules/dummy", async (CreateScheduleV2Request request, ISender sender) =>
        {
            var command = new CreateScheduleCommand(request.PatientId, request.DoctorId);
            var result = await sender.Send(command);
            return Results.Ok(new CreateScheduleV2Response(result.ScheduleId));
        })
        .WithName("CreateSchedule v2")
        .WithTags("Schedules Version 2")
        .Produces<CreateScheduleV2Response>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Create a new schedule")
        .WithSummary("Create a new schedule");
    }
}