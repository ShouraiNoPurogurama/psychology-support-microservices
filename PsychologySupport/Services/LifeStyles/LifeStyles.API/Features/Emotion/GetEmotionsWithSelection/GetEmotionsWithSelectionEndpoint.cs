using BuildingBlocks.Pagination;
using Carter;
using LifeStyles.API.Dtos.Emotions;
using LifeStyles.API.Extensions;
using MediatR;

namespace LifeStyles.API.Features.Emotion.GetEmotionsWithSelection;

public record GetEmotionsWithSelectionRequest(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = null,
    string SortBy = "name",
    string SortOrder = "asc");

public record GetEmotionsWithSelectionResponse(PaginatedResult<EmotionWithSelectionDto> Emotions);

public class GetEmotionsWithSelectionEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/me/emotions", async (
                [AsParameters] GetEmotionsWithSelectionRequest request,
                ISender sender, HttpContext httpContext) =>
            {
                var profileId = httpContext.User.GetProfileId();

                var query = new GetEmotionsWithSelectionQuery(
                    profileId,
                    new PaginationRequest(request.PageIndex, request.PageSize),
                    request.Search,
                    request.SortBy,
                    request.SortOrder);

                var result = await sender.Send(query);

                var response = new GetEmotionsWithSelectionResponse(result.Emotions);
                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetEmotionsWithSelection")
            .WithTags("Emotions")
            .WithSummary("Get all emotions with selection marker for the current profile")
            .WithDescription("Returns a paginated list of all available emotions, each with a marker indicating whether it is currently selected by the authenticated user (profile).")
            .Produces<GetEmotionsWithSelectionResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }
}