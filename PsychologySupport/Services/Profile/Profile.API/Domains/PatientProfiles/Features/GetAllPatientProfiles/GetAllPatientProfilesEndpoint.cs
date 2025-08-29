using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using Profile.API.Common.Helpers;
using Profile.API.Domains.PatientProfiles.Dtos;

namespace Profile.API.Domains.PatientProfiles.Features.GetAllPatientProfiles;

public record GetAllPatientProfilesResponse(PaginatedResult<GetPatientProfileDto> PaginatedResult);

public class GetAllPatientProfilesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/patients", async ([AsParameters] GetAllPatientProfilesQuery request, ISender sender, HttpContext httpContext) =>
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
            .Produces<GetAllPatientProfilesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Get All Patient Profiles")
            .WithSummary("Get All Patient Profiles");
    }
}