using BuildingBlocks.Pagination;
using Profile.API.Common.Helpers;
using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetAllPatientProfiles;

public record GetAllPatientProfilesResponse(PaginatedResult<GetPatientProfileDto> PaginatedResult);

public class GetAllPatientProfilesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/patients", async ([AsParameters] GetAllPatientProfilesQuery request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var result = await sender.Send(request);

                var response = result.Adapt<GetAllPatientProfilesResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("Manager", "Admin"))
            .WithName("GetAllPatientProfiles")
            .WithTags("PatientProfiles")
            .Produces<GetAllPatientProfilesResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Retrieves a paginated list of all patient profiles. Requires 'Manager' or 'Admin' role. Returns paginated profile data.")
            .WithSummary("Get paginated patient profiles");
    }
}