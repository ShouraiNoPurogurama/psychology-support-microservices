using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Domains.Public.Extensions;

namespace Profile.API.Domains.Public.PatientProfiles.Features.OnboardingPatientProfile;

public record OnboardingPatientProfileRequest(
    Guid JobId
);

public record OnboardingProfileResponse(bool IsSuccess);

public class OnboardingPatientProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("patients/onboarding",
                async ([FromBody] OnboardingPatientProfileRequest request,
                    ClaimsPrincipal user, ISender sender) =>
                {
                    var patientId = user.GetPatientId();

                    var command = request.Adapt<OnboardingPatientProfileCommand>() with
                    {
                        PatientId = patientId
                    };
                    var result = await sender.Send(command);
                    
                    var response = result.Adapt<OnboardingProfileResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User"))
            .WithName("OnboardingPatientProfile")
            .WithTags("PatientProfiles")
            .Produces<OnboardingProfileResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Onboarding Patient Profile")
            .WithSummary("Onboarding Patient Profile");
    }
}