using BuildingBlocks.Exceptions;
using Carter;
using LifeStyles.API.Common;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace LifeStyles.API.Features02.PatientTherapeuticActivity.GetPatientTherapeuticActivity;

public record GetPatientTherapeuticActivityV2Request(Guid PatientProfileId);

public record GetPatientTherapeuticActivityV2Response(IEnumerable<Models.PatientTherapeuticActivity> PatientTherapeuticActivities);

public class GetPatientTherapeuticActivityV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v2/me/{patientProfileId:guid}/therapeuticActivities",
                async (HttpContext httpContext, Guid patientProfileId, ISender sender) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasAccessToPatientProfile(patientProfileId, httpContext.User))
                        throw new ForbiddenException();
                    
                    var query = new GetPatientTherapeuticActivityQuery(patientProfileId);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetPatientTherapeuticActivityV2Response>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetPatientTherapeuticActivities v2")
            .WithTags("PatientTherapeuticActivities Version 2")
            .Produces<GetPatientTherapeuticActivityV2Response>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Patient Therapeutic Activities")
            .WithSummary("Get Patient Therapeutic Activities");
    }
}