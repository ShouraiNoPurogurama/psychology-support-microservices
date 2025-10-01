using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Wellness.Application.Features.ModuleSections.Dtos;

public record GetModuleSectionsRequest(
    Guid WellnessModuleId,
    Guid SubjectRef,
    int PageIndex = 1,
    int PageSize = 10,
    string? TargetLang = null
)
{
    public PaginationRequest ToPaginationRequest() => new(PageIndex, PageSize);
}

public record GetModuleSectionsResponse(PaginatedResult<ModuleSectionDto> Sections);

public class GetModuleSectionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/wellness-modules/{WellnessModuleId}/module-sections", async (
                [AsParameters] GetModuleSectionsRequest request,
                ISender sender, HttpContext httpContext) =>
        {
            // Optional: Authorization check
            //if (!AuthorizationHelpers.CanView(request.SubjectRef, httpContext.User))
            //    throw new ForbiddenException();

            var query = new GetModuleSectionsQuery(
                request.WellnessModuleId,
                request.SubjectRef,
                request.ToPaginationRequest(),
                request.TargetLang
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetModuleSectionsResponse(result.Sections));
        })
        .WithName("GetModuleSections")
        .WithTags("ModuleSections")
        .Produces<GetModuleSectionsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated list of ModuleSections with MediaUrl and optional translation")
        .WithDescription("Returns ModuleSections for a WellnessModule. MediaUrl is fetched via Media Service, translation via Translation Service.");
    }
}
