using Profile.API.Common.Helpers;
using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetPatientProfile;

public record GetPatientProfileRequest(Guid Id);

public record GetPatientProfileResponse(GetPatientProfileDto PatientProfileDto);

public class GetPatientProfileEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients/{id:guid}", async (Guid id, ISender sender, HttpContext httpContext) =>
            {
                if (!AuthorizationHelpers.CanViewPatientProfile(id, httpContext.User))
                    throw new ForbiddenException();
                
                var query = new GetPatientProfileQuery(id);
                var result = await sender.Send(query);
                var response = result.Adapt<GetPatientProfileResponse>();
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin", "Manager"))
            .WithName("GetPatientProfile")
            .WithTags("PatientProfiles")
            .Produces<GetPatientProfileResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves the patient profile for the specified ID. Requires 'User', 'Admin', or 'Manager' role. Returns profile details.")
            .WithSummary("Get patient profile by ID");
    }
}