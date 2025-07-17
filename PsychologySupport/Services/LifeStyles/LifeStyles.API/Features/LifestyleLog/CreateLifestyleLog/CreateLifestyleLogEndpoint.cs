using BuildingBlocks.Enums;
using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
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
            async (HttpContext httpContext, [FromBody] CreateLifestyleLogRequest request, ISender sender) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasAccessToPatientProfile(request.PatientProfileId, httpContext.User))
                    throw new ForbiddenException();
                
                // Send command
                var command = new CreateLifestyleLogCommand(
                    request.PatientProfileId,
                    request.LogDate,
                    request.SleepHours,
                    request.ExerciseFrequency,
                    request.AvailableTimePerDay
                );

                await sender.Send(command);

                // Prepare response
                var response = new CreateLifestyleLogResponse(
                    request.PatientProfileId,
                    request.LogDate,
                    request.SleepHours,
                    request.ExerciseFrequency,
                    request.AvailableTimePerDay
                );

                return Results.Created($"/lifestyle-logs/{request.PatientProfileId}", response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreateLifestyleLog")
            .WithTags("LifestyleLogs")
            .Produces<CreateLifestyleLogResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden)
            .WithSummary("Create a Lifestyle Log")
            .WithDescription("Create a Lifestyle Log for a patient.");
    }
}
