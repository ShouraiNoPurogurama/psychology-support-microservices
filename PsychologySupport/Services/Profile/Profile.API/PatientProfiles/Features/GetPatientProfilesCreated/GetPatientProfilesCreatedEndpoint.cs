using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Common.Helpers;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Features.GetPatientProfilesCreated;

public record GetPatientProfilesCreatedEndpointRequest(
    int PageIndex = 1,
    int PageSize = 10,
    DateTime? StartDate = null,
    DateTime? EndDate = null
);

public record GetPatientProfilesCreatedEndpointResponse(
    IEnumerable<GetCreatedPatientProfileDto> Datapoints
);

public class GetPatientProfilesCreatedEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("patients/created", async (
                [AsParameters] GetPatientProfilesCreatedEndpointRequest request,
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User) && !AuthorizationHelpers.IsExclusiveAccess(httpContext.User))
                    throw new ForbiddenException();
                
                var query = new GetPatientProfilesCreatedQuery(
                    new PaginationRequest(
                        request.PageIndex,
                        request.PageSize),
                    request.StartDate,
                    request.EndDate);
                
                var result = await sender.Send(query);
                
                var response = result.Adapt<GetPatientProfilesCreatedEndpointResponse>();
                
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin", "Manager"))
            .WithName("GetPatientProfilesCreated")
            .WithTags("Dashboard")
            .Produces<GetPatientProfilesCreatedEndpointResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Get Created Patient Profiles")
            .WithSummary("Get Created Patient Profiles");
    }
}