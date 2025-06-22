using Carter;
using LifeStyles.API.Dtos.EmotionSelections;
using LifeStyles.API.Dtos.PatientEmotionCheckpoints;
using Mapster;
using MediatR;
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
                async ([FromRoute] Guid patientProfileId, [FromBody] CreatePatientEmotionCheckpointRequest request, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var command = new CreatePatientEmotionCheckpointCommand(
                        patientProfileId,
                        request.Emotions,
                        request.LogDate);

                    var result = await sender.Send(command, cancellationToken);

                    var response = result.Adapt<CreatePatientEmotionCheckpointResponse>();

                    return Results.Ok(response);
                })
            .WithName("CreatePatientEmotionCheckpoint")
            .WithTags("PatientEmotionCheckpoint")
            .WithDescription("Creates a new patient emotion checkpoint.")
            .Produces<CreatePatientEmotionCheckpointResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}