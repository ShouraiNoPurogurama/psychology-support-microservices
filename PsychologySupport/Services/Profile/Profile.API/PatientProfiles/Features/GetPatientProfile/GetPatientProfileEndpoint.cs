using Carter;
using Mapster;
using Profile.API.Common.Helpers;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetPatientProfile;

public record GetPatientProfileRequest(Guid Id);

public record GetPatientProfileResponse(GetPatientProfileDto PatientProfileDto);

public class GetPatientProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{id:guid}", async (Guid id, ISender sender, HttpContext httpContext) =>
            {
                if (!AuthorizationHelpers.CanViewPatientProfile(id, httpContext.User) && !AuthorizationHelpers.IsExclusiveAccess(httpContext.User))
                    throw new ForbiddenException();
                
                var query = new GetPatientProfileQuery(id);
                var result = await sender.Send(query);
                var response = result.Adapt<GetPatientProfileResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Manager"))
            .WithName("GetPatientProfile")
            .WithTags("PatientProfiles")
            .Produces<GetPatientProfileResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Get Patient Profile")
            .WithSummary("Get Patient Profile");
    }
}