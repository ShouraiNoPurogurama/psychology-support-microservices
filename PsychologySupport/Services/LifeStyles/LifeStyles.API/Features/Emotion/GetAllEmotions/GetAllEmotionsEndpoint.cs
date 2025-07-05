using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Common;
using LifeStyles.API.Dtos.Emotions;
using MediatR;

namespace LifeStyles.API.Features.Emotion.GetAllEmotions;

public record GetAllEmotionsRequest(int PageIndex = 1,
    int PageSize = 10,
    string? Search = null, 
    string SortBy = "name", 
    string SortOrder = "asc");

public record GetAllEmotionsResponse(PaginatedResult<EmotionDto> Emotions);

public class GetAllEmotionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/emotions", async (
            [AsParameters] GetAllEmotionsRequest request,
            ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                return Results.Problem(
                           statusCode: StatusCodes.Status403Forbidden,
                           title: "Forbidden",
                           detail: "You do not have permission to access this resource."
                       );

            var query = new GetAllEmotionsQuery(
                new PaginationRequest(request.PageIndex, request.PageSize),
                request.Search,
                request.SortBy,
                request.SortOrder);

            var result = await sender.Send(query);
            var response = new GetAllEmotionsResponse(result.Emotions);
            return Results.Ok(response);
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("GetEmotions")
        .WithTags("Emotions")
        .WithSummary("Get all emotions")
        .WithDescription("Returns a paginated list of all available emotions")
        .Produces<GetAllEmotionsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);
    }
}