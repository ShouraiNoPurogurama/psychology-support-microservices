using Carter;
using LifeStyles.API.Dtos.PatientEmotionCheckpoints;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientEmotionCheckpoint.GetPatientEmotionCheckpoint;

public class GetPatientEmotionCheckpointEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{patientProfileId:guid}/emotion-checkpoints",
                async ([FromRoute] Guid patientProfileId, [FromQuery] DateTime? date, ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var query = new GetPatientEmotionCheckpointQuery(patientProfileId, date);

                    var result = await sender.Send(query, cancellationToken);

                    return Results.Ok(result);
                })
            .WithName("GetPatientEmotionCheckpoint")
            .WithTags("PatientEmotionCheckpoint")
            .WithDescription("Gets a specific patient emotion checkpoint by ID.")
            .Produces<PatientEmotionCheckpointDto>()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}