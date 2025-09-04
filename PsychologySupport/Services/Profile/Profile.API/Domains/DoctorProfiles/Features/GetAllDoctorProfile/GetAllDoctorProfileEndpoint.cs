using BuildingBlocks.Pagination;
using Profile.API.Domains.DoctorProfiles.Dtos;

namespace Profile.API.Domains.DoctorProfiles.Features.GetAllDoctorProfile;

public record GetAllDoctorProfilesResponse(PaginatedResult<DoctorProfileDto> DoctorProfiles);

public class GetAllDoctorProfilesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/doctors", async ([AsParameters] GetAllDoctorProfilesQuery request, ISender sender) =>
        {
            var result = await sender.Send(request);
            var response = result.Adapt<GetAllDoctorProfilesResponse>();
            return Results.Ok(response);
        })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin", "Manager","Doctor"))
            .WithName("GetAllDoctorProfiles")
            .WithTags("DoctorProfiles")
            .Produces<GetAllDoctorProfilesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("Get All Doctor Profiles")
            .WithSummary("Get All Doctor Profiles");
    }
}