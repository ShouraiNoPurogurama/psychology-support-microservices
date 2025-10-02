using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Wellness.API.Common;
using Wellness.Application.Features.ModuleSections.Dtos;

public record GetWellnessModulesRequest(
    int PageIndex = 1,
    int PageSize = 10,
    string? TargetLang = null
)
{
    public PaginationRequest ToPaginationRequest() => new(PageIndex, PageSize);
}

public record GetWellnessModulesResponse(PaginatedResult<WellnessModuleDto> Modules);

public class GetWellnessModuleEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/wellness-modules", async (
                [AsParameters] GetWellnessModulesRequest request,
                ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            //if (!AuthorizationHelpers.HasViewAccess(httpContext.User))
            //    throw new ForbiddenException();

            var query = new GetWellnessModulesQuery(request.ToPaginationRequest(), request.TargetLang);
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
