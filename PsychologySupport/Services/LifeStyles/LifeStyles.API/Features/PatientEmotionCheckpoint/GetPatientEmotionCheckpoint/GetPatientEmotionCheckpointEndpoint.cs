using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos.PatientEmotionCheckpoints;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LifeStyles.API.Features.PatientEmotionCheckpoint.GetPatientEmotionCheckpoint;

public class GetPatientEmotionCheckpointEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{patientProfileId:guid}/emotion-checkpoints",
            async (HttpContext httpContext, [FromRoute] Guid patientProfileId, 
            [FromQuery] DateTimeOffset? date, ISender sender,CancellationToken cancellationToken) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                    return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

                var query = new GetPatientEmotionCheckpointQuery(patientProfileId, date);
                var result = await sender.Send(query, cancellationToken);

                return Results.Ok(result);
            })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("GetPatientEmotionCheckpoint")
        .WithTags("PatientEmotionCheckpoint")
        .WithDescription("Gets a specific patient emotion checkpoint by ID.")
        .Produces<PatientEmotionCheckpointDto>()
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}
