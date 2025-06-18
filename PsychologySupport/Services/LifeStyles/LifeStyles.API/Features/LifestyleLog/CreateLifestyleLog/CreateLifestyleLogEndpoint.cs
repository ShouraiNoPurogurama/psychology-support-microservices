using BuildingBlocks.Enums;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.LifestyleLog.CreateLifestyleLog;

public record CreateLifestyleLogRequest(
    Guid PatientProfileId,
    DateTimeOffset LogDate,
    SleepHoursLevel SleepHours,
    ExerciseFrequency ExerciseFrequency,
    AvailableTimePerDay AvailableTimePerDay
);

public record CreateLifestyleLogResponse(
    Guid PatientProfileId,
    DateTimeOffset LogDate,
    SleepHoursLevel SleepHours,
    ExerciseFrequency ExerciseFrequency,
    AvailableTimePerDay AvailableTimePerDay
);

public class CreateLifestyleLogEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("lifestyle-logs",
            async ([FromBody] CreateLifestyleLogRequest request, ISender sender) =>
            {
                var command = new CreateLifestyleLogCommand(
                    request.PatientProfileId,
                    request.LogDate,
                    request.SleepHours,
                    request.ExerciseFrequency,
                    request.AvailableTimePerDay
                );

                await sender.Send(command);

                var response = new CreateLifestyleLogResponse(
                    request.PatientProfileId,
                    request.LogDate,
                    request.SleepHours,
                    request.ExerciseFrequency,
                    request.AvailableTimePerDay
                );

                return Results.Created($"/lifestyle-logs/{request.PatientProfileId}", response);
            })
            .WithName("CreateLifestyleLog")
            .WithTags("LifestyleLogs")
            .Produces<CreateLifestyleLogResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Create a Lifestyle Log")
            .WithDescription("Create a Lifestyle Log");
    }
}
