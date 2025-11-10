using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Wellness.API.Common.Authentication;
using Wellness.API.Common.Subscription;
using Wellness.Application.Features.ModuleSections.Dtos;
using Wellness.Application.Features.ModuleSections.Queries;
using Wellness.Domain.Aggregates.ModuleSections.Enums;

public record GetModuleSectionsWithArticlesRequest(
    Guid? ModuleSectionId,
    int PageIndex = 1,
    int PageSize = 10,
    string? TargetLang = null,
    ModuleCategory? Category = null
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
                ICurrentUserSubscriptionAccessor subscriptionAccessor, 
                ISender sender
            ) =>
        {
            var subjectRef = currentActor.GetRequiredSubjectRef();
            bool isFreeTier = subscriptionAccessor.IsFreeTier();

            var query = new GetModuleSectionsWithArticlesQuery(
                ModuleSectionId: request.ModuleSectionId,
                SubjectRef: subjectRef,
                PaginationRequest: request.ToPaginationRequest(),
                TargetLang: request.TargetLang,
                Category: request.Category,
                IsFreeTier: isFreeTier 
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetModuleSectionsWithArticlesResponse(result.Sections));
        })
        .RequireAuthorization()
        .WithName("GetModuleSectionsWithArticles")
        .WithTags("ModuleSections")
        .Produces<GetModuleSectionsWithArticlesResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound);


        app.MapGet("/v1/me/module-sections", async (
              [AsParameters] GetModuleSectionsWithArticlesRequest request,
              ICurrentActorAccessor currentActor,
              ICurrentUserSubscriptionAccessor subscriptionAccessor,
              ISender sender
          ) =>
        {
            var subjectRef = currentActor.GetRequiredSubjectRef();
            bool isFreeTier = subscriptionAccessor.IsFreeTier();

            var query = new GetModuleSectionsWithArticlesQuery(
                ModuleSectionId: null,
                SubjectRef: subjectRef,
                PaginationRequest: request.ToPaginationRequest(),
                TargetLang: request.TargetLang,
                Category: request.Category,
                IsFreeTier: isFreeTier
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetModuleSectionsWithArticlesResponse(result.Sections));
        })
        .RequireAuthorization()
        .WithName("GetModuleSectionsList")
        .WithTags("ModuleSections")
        .Produces<GetModuleSectionsWithArticlesResponse>();
    }
}
