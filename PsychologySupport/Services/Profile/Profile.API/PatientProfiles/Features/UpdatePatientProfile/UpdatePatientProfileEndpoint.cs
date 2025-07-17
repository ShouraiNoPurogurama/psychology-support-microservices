using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Common.Helpers;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.UpdatePatientProfile;

public record UpdatePatientProfileRequest(UpdatePatientProfileDto PatientProfileUpdate);
public record UpdatePatientProfileResponse(Guid Id);

public class UpdatePatientProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("patients/{id:guid}",
            async ([FromRoute] Guid id,
                   [FromBody] UpdatePatientProfileRequest request,
                   ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(id, httpContext.User))
                    throw new ForbiddenException();

                var command = new UpdatePatientProfileCommand(id, request.PatientProfileUpdate);
                var result = await sender.Send(command);
                var response = result.Adapt<UpdatePatientProfileResponse>();

                return Results.Ok(response);
            })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("UpdatePatientProfile")
        .WithTags("PatientProfiles")
        .Produces<UpdatePatientProfileResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Update Patient Profile")
        .WithSummary("Update Patient Profile");
    }
}
