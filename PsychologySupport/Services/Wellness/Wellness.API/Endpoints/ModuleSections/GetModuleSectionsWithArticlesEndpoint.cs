using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Wellness.Application.Features.ModuleSections.Dtos;

public record GetModuleSectionsWithArticlesRequest(
    Guid ModuleId,
    Guid SubjectRef,
    int PageIndex = 1,
    int PageSize = 10
);

public record GetModuleSectionsWithArticlesResponse(PaginatedResult<ModuleSectionDetailsDto> Sections);

public class GetModuleSectionsWithArticlesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/module-sections-with-articles/{ModuleId}", async (
                [AsParameters] GetModuleSectionsWithArticlesRequest request,
                ISender sender) =>
        {
            var query = request.Adapt<GetModuleSectionsWithArticlesQuery>();
            var result = await sender.Send(query);

            return Results.Ok(new GetModuleSectionsWithArticlesResponse(result.Sections));
        })
        .WithName("GetModuleSectionsWithArticles")
        .WithTags("ModuleSections")
        .Produces<GetModuleSectionsWithArticlesResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated ModuleSections with nested SectionArticles and progress")
        .WithDescription("Returns paginated ModuleSections filtered by ModuleId. Each section includes SectionArticles with Completed status for SubjectRef and MediaUrl from Media Service.");
    }
}
