using BuildingBlocks.Pagination;
using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetAllIndustry;

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
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin", "Manager"))
        .WithName("GetAllIndustries")
        .WithTags("Industries")
        .Produces<GetAllIndustryResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithDescription("Retrieves a paginated list of all industries. Requires 'User', 'Admin', or 'Manager' role.")
        .WithSummary("Get paginated industries");
    }
}
