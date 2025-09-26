using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Wellness.Application.Features.ModuleSections.Dtos;

public record GetModuleSectionsRequest(
    Guid WellnessModuleId,
    Guid SubjectRef,
    int PageIndex = 1,
    int PageSize = 10
);

public record GetModuleSectionsResponse(PaginatedResult<ModuleSectionDto> Sections);

public class GetModuleSectionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/module-sections/{WellnessModuleId}", async (
                [AsParameters] GetModuleSectionsRequest request,
                ISender sender) =>
        {
            // Thiếu validate SubjectRef

            var query = request.Adapt<GetModuleSectionsQuery>();
            var result = await sender.Send(query);

            return Results.Ok(new GetModuleSectionsResponse(result.Sections));
        })
        .WithName("GetModuleSections")
        .WithTags("ModuleSections")
        .Produces<GetModuleSectionsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated list of ModuleSections by ModuleId with MediaUrl and Subject progress")
        .WithDescription("Returns paginated ModuleSections filtered by ModuleId. MediaUrl is fetched via Media Service, and progress for SubjectRef is included.");
    }
}
