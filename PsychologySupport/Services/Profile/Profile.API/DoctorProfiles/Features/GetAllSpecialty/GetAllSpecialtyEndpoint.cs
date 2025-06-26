using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Features.GetAllSpecialty
{
    public record GetAllSpecialtiesResponse(IEnumerable<SpecialtyDto> Specialties);

    public class GetAllSpecialtyEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/specialties", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var query = new GetAllSpecialtiesQuery(request);
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllSpecialtiesResponse>();

                return Results.Ok(response);
            })
                .RequireAuthorization(policy => policy.RequireRole("Doctor", "Admin","User","Manager"))
                .WithName("GetAllSpecialties")
                .WithTags("Specialties")
                .Produces<GetAllSpecialtiesResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Get All Specialties")
                .WithSummary("Get All Specialties");
        }
    }
}

