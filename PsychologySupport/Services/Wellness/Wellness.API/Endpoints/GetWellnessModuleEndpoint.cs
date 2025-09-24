using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Wellness.Application.Features.ModuleSections.Dtos;

public record GetWellnessModulesRequest(
    int PageIndex = 1,
    int PageSize = 10
);

public record GetWellnessModulesResponse(PaginatedResult<WellnessModuleDto> Modules);

public class GetWellnessModuleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/wellness-modules", async (
                [AsParameters] GetWellnessModulesRequest request,
                ISender sender) =>
        {
            // Thiếu validate SubjectRef

            var query = request.Adapt<GetWellnessModulesQuery>();
            var result = await sender.Send(query);

            return Results.Ok(new GetWellnessModulesResponse(result.Modules));
        })
        .WithName("GetWellnessModules")
        .WithTags("WellnessModules")
        .Produces<GetWellnessModulesResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated list of WellnessModules with MediaUrl")
        .WithDescription("Returns paginated WellnessModules. MediaUrl is fetched via Media Service.");
    }
}
