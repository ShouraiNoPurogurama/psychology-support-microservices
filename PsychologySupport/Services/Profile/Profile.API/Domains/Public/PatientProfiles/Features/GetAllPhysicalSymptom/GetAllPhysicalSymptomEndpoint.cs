using BuildingBlocks.Pagination;
using Profile.API.Domains.Public.PatientProfiles.Dtos;

namespace Profile.API.Domains.Public.PatientProfiles.Features.GetAllPhysicalSymptom
{
    public record GetAllPhysicalSymptomResponse(PaginatedResult<PhysicalSymptomDto> PhysicalSymptom);

    public class GetAllPhysicalSymptomEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/physical-symptoms", async (
                [AsParameters] GetAllPhysicalSymptomQuery request, ISender sender) =>
            {
                var result = await sender.Send(request);
                var response = result.Adapt<GetAllPhysicalSymptomResponse>();

                return Results.Ok(response);
            })
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
                .WithName("GetAllPhysicalSymptoms")
                .WithTags("PhysicalSymptoms")
                .Produces<GetAllPhysicalSymptomResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Retrieves a paginated list of all physical symptoms for the authenticated user. Requires 'User' or 'Admin' role.")
                .WithSummary("Get paginated physical symptoms");
        }
    }
}
