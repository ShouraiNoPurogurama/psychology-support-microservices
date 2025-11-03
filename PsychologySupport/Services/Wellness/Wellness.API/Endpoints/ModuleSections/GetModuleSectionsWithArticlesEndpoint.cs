using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Wellness.API.Common.Authentication;
using Wellness.Application.Features.ModuleSections.Dtos;

public record GetModuleSectionsWithArticlesRequest(
    Guid ModuleSectionId,
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
        app.MapGet("/v1/me/module-sections/{ModuleSectionId}", async (
                [AsParameters] GetModuleSectionsWithArticlesRequest request,
                ICurrentActorAccessor currentActor,  
                ISender sender
            ) =>
        {
            var subjectRef = currentActor.GetRequiredSubjectRef();

            var query = new GetModuleSectionsWithArticlesQuery(
                ModuleSectionId: request.ModuleSectionId,
                SubjectRef: subjectRef,
                PaginationRequest: request.ToPaginationRequest(),
                TargetLang: request.TargetLang
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetModuleSectionsWithArticlesResponse(result.Sections));
        })
        .RequireAuthorization()
        .WithName("GetModuleSectionsWithArticles")
        .WithTags("ModuleSections")
        .Produces<GetModuleSectionsWithArticlesResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated ModuleSections with nested SectionArticles, progress, and translation for the current user")
        .WithDescription("Returns paginated ModuleSections filtered by ModuleId for the authenticated user. Each section includes SectionArticles with progress, MediaUrl, and optional translation.");
    }
}
