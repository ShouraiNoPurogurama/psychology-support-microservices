using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Features.GetDoctorProfileBySpecialities;

public record GetDoctorsBySpecialitiesResponse(PaginatedResult<DoctorProfileDto> DoctorProfiles);

public class GetDoctorProfileBySpecialitiesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/doctors/specialties",
                async ([FromQuery] string specialties, [AsParameters] PaginationRequest request, ISender sender) =>
                {
                    var query = new GetDoctorProfileBySpecialitiesQuery(specialties, request);
                    var result = await sender.Send(query);
                    var response = result.Adapt<GetDoctorsBySpecialitiesResponse>();

                    return Results.Ok(response);
                })
            .WithName("GetDoctorsBySpecialities")
            .Produces<GetDoctorsBySpecialitiesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Doctor Profiles by Specialties")
            .WithSummary("Get Doctor Profiles by Specialties");
    }
}