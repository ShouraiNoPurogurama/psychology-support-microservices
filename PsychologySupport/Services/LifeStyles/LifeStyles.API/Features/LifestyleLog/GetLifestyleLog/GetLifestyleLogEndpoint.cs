using Carter;
using Mapster;
using MediatR;
using LifeStyles.API.Dtos;

namespace LifeStyles.API.Features.LifestyleLog.GetLifestyleLog;

public record GetLifestyleLogRequest(Guid PatientProfileId);

public record GetLifestyleLogResponse(LifestyleLogDto LifestyleLog);

public class GetLifestyleLogEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/lifestyle-logs/{patientProfileId:guid}",
                async (Guid patientProfileId, ISender sender) =>
                {
                    var query = new GetLifestyleLogQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetLifestyleLogResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetLifestyleLog")
            .WithTags("LifestyleLogs")
            .WithSummary("Get most recent Lifestyle Log")
            .WithDescription("GetLifestyleLog")
            .Produces<GetLifestyleLogResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }
}
