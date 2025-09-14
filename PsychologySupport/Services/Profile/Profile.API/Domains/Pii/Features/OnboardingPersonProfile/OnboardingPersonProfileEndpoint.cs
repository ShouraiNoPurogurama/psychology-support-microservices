using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Domains.Pii.Extensions;

namespace Profile.API.Domains.Pii.Features.OnboardingPersonProfile;

public record OnboardingPersonProfileRequest(
    UserGender Gender,
    DateOnly BirthDate,
    string Address
);

public record OnboardingPersonProfileResponse(bool IsSuccess);

public class OnboardingPersonProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("pii/me/onboarding",
                async ([FromBody] OnboardingPersonProfileRequest onboardingRequest, ClaimsPrincipal user, ISender sender) =>
                {
                    var subjectRef = user.GetSubjectRef();

                    var command = onboardingRequest.Adapt<OnboardingPersonProfileCommand>() with
                    {
                        SubjectRef = subjectRef
                    };

                    var result = await sender.Send(command);

                    var response = result.Adapt<OnboardingPersonProfileResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization()
            .WithName("OnboardingPersonProfile")
            .WithTags("Person Profiles")
            .Produces<OnboardingPersonProfileResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Onboarding Person Profile")
            .WithSummary("Onboarding Person Profile");
    }
}