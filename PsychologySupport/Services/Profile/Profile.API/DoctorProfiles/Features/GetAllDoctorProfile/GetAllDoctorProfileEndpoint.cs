using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Features.GetAllDoctorProfile;

public record GetAllDoctorProfilesResponse(IEnumerable<DoctorProfileDto> DoctorProfileDtos);

public class GetAllDoctorProfilesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/doctors", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var query = new GetAllDoctorProfilesQuery(request);
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllDoctorProfilesResponse>();
                return Results.Ok(response);
            })
            .WithName("GetAllDoctorProfiles")
            .Produces<GetAllDoctorProfilesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithDescription("GetAll Doctor Profile")
            .WithSummary("GetAll Doctor Profile");
    }
}