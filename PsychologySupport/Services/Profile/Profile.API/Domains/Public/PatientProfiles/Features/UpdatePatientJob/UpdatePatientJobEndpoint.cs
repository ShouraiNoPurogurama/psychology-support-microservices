using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Extensions;

namespace Profile.API.Domains.Public.PatientProfiles.Features.UpdatePatientJob;

public record UpdatePatientJobResponse(Guid Id);

public class UpdatePatientJobEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("patients/me/jobs",
                async ([FromBody] Guid jobId, ClaimsPrincipal user,
                    ISender sender) =>
                {
                    var patientId = user.GetPatientId();
                    
                    var command = new UpdatePatientJobCommand(patientId, jobId);
                    var result = await sender.Send(command);
                    var response = result.Adapt<UpdatePatientJobResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization()
            .WithName("UpdatePatientJob")
            .WithTags("Jobs")
            .Produces<UpdatePatientJobResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Updates the job for the authenticated patient. Requires authorization. Returns the updated job ID on success.")
            .WithSummary("Update patient job");
    }
}