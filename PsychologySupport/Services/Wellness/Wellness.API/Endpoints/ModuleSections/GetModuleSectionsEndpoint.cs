using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common.Authentication;
using Wellness.Application.Features.ModuleSections.Dtos;
using BuildingBlocks.Exceptions;

public record GetModuleSectionsRequest(
    Guid WellnessModuleId,
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
                ICurrentActorAccessor currentActor,  
                ISender sender
            ) =>
        {

            var subjectRef = currentActor.GetRequiredSubjectRef();

            var query = new GetModuleSectionsQuery(
                WellnessModuleId: request.WellnessModuleId,
                SubjectRef: subjectRef,
                PaginationRequest: request.ToPaginationRequest(),
                TargetLang: request.TargetLang
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetModuleSectionsResponse(result.Sections));
        })
        .RequireAuthorization() 
        .WithName("GetModuleSections")
        .WithTags("ModuleSections")
        .Produces<GetModuleSectionsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated list of ModuleSections for current user")
        .WithDescription("Returns ModuleSections for the specified WellnessModule of the authenticated user.");
    }
}
