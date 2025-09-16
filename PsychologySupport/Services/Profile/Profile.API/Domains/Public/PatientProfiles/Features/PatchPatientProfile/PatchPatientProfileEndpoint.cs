using Microsoft.AspNetCore.Mvc;
using Profile.API.Common.Helpers;
using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Features.PatchPatientProfile;

public record PatchPatientProfileRequest(PatchPatientProfileDto PatientProfile);
public record PatchPatientProfileResponse(bool IsSuccess);

public class PatchPatientProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("patients/{id:guid}",
            async ([FromRoute] Guid id,
                   [FromBody] PatchPatientProfileRequest request,
                   ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(id, httpContext.User))
                    throw new ForbiddenException();

                var command = new PatchPatientProfileCommand(id, request.PatientProfile);
                var result = await sender.Send(command);
                var response = result.Adapt<PatchPatientProfileResponse>();

                return Results.Ok(response);
            })
        // .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("PatchPatientProfile")
        .WithTags("PatientProfiles")
        .Produces<PatchPatientProfileResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Updates the patient profile with the provided data. Requires 'User' or 'Admin' role. Returns success status.")
        .WithSummary("Patch patient profile");
    }
}
