using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using Profile.API.Domains.PatientProfiles.Dtos;

namespace Profile.API.Domains.PatientProfiles.Features.GetAllIndustry;

public record GetAllIndustryResponse(PaginatedResult<IndustryDto> Industries);

public class GetAllIndustryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/industries", async ([AsParameters] GetAllIndustryQuery request, ISender sender) =>
        {
            var result = await sender.Send(request);
            var response = result.Adapt<GetAllIndustryResponse>();

            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Manager"))
        .WithName("GetAllIndustries")
        .WithTags("PatientProfiles")
        .Produces<GetAllIndustryResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Get all industries")
        .WithSummary("Get all industries");
    }
}
