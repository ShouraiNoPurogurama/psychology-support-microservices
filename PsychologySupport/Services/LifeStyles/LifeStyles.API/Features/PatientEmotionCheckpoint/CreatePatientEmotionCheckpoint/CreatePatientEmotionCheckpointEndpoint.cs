using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos.EmotionSelections;
using LifeStyles.API.Dtos.PatientEmotionCheckpoints;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientEmotionCheckpoint.CreatePatientEmotionCheckpoint;

public record CreatePatientEmotionCheckpointRequest(
    List<CreateEmotionSelectionDto> Emotions,
    DateTimeOffset LogDate);

public record CreatePatientEmotionCheckpointResponse(
    PatientEmotionCheckpointDto CheckpointDto);

public class CreatePatientEmotionCheckpointEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/patients/{patientProfileId:guid}/emotion-checkpoints",
                async (HttpContext httpContext,[FromRoute] Guid patientProfileId, [FromBody] CreatePatientEmotionCheckpointRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                        return Results.Forbid();

                    var command = new CreatePatientEmotionCheckpointCommand(
                        patientProfileId,
                        request.Emotions,
                        request.LogDate);

                    var result = await sender.Send(command, cancellationToken);

                    var response = result.Adapt<CreatePatientEmotionCheckpointResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreatePatientEmotionCheckpoint")
            .WithTags("PatientEmotionCheckpoint")
            .WithDescription("Creates a new patient emotion checkpoint.")
            .Produces<CreatePatientEmotionCheckpointResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}