using BuildingBlocks.Pagination;
using Microsoft.AspNetCore.Mvc;
using Profile.API.Domains.Public.DoctorProfiles.Dtos;

namespace Profile.API.Domains.Public.DoctorProfiles.Features.GetDoctorProfileBySpecialities;

public record GetDoctorsBySpecialitiesResponse(PaginatedResult<DoctorProfileDto> DoctorProfiles);

public class GetDoctorProfileBySpecialitiesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/doctors/specialties",
                async ([FromQuery] string specialties, [AsParameters] PaginationRequest request, ISender sender) =>
                {
                    var query = new GetDoctorProfileBySpecialitiesQuery(specialties, request);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetDoctorsBySpecialitiesResponse>();

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Manager"))
            .WithName("GetDoctorsBySpecialities")
            .WithTags("DoctorProfiles")
            .Produces<GetDoctorsBySpecialitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Doctor Profiles by Specialties")
            .WithSummary("Get Doctor Profiles by Specialties");
    }
}