using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Wellness.Application.Features.ModuleSections.Dtos;

public record GetModuleSectionsWithArticlesRequest(
    Guid ModuleId,
    Guid SubjectRef,
    int PageIndex = 1,
    int PageSize = 10,
    string? TargetLang = null
)
{
    public PaginationRequest ToPaginationRequest() => new(PageIndex, PageSize);
}

public record GetModuleSectionsWithArticlesResponse(PaginatedResult<ModuleSectionDetailsDto> Sections);

public class GetModuleSectionsWithArticlesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/module-sections/{ModuleId}", async (
                [AsParameters] GetModuleSectionsWithArticlesRequest request,
                ISender sender, HttpContext httpContext) =>
        {
            var query = new GetModuleSectionsWithArticlesQuery(
                request.ModuleId,
                request.SubjectRef,
                request.ToPaginationRequest(),
                request.TargetLang
            );

            var result = await sender.Send(query);
            return Results.Ok(new GetModuleSectionsWithArticlesResponse(result.Sections));
        })
        .WithName("GetModuleSectionsWithArticles")
        .WithTags("ModuleSections")
        .Produces<GetModuleSectionsWithArticlesResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated ModuleSections with nested SectionArticles, progress, and translation")
        .WithDescription("Returns paginated ModuleSections filtered by ModuleId. Each section includes SectionArticles with Completed status for SubjectRef, MediaUrl from Media Service, and translation if TargetLang is provided.");
    }
}
