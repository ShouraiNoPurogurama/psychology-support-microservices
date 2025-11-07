using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Application.Features.Challenges.Queries;
using Wellness.Domain.Aggregates.Challenges.Enums;

namespace Wellness.API.Endpoints.Challenges;

public record GetChallengesRequest(
    ChallengeType? ChallengeType,
    ImprovementTag? ImprovementTag,
    int PageIndex = 1,
    int PageSize = 10,
    string? TargetLang = null
);

public record GetChallengesResponse(PaginatedResult<ChallengeDto> Challenges);

public class GetChallengesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/challenges", async (
            [AsParameters] GetChallengesRequest request,
            ISender sender) =>
        {
            var query = new GetChallengesQuery(
                request.ChallengeType,
                request.ImprovementTag,
                new PaginationRequest(request.PageIndex, request.PageSize),
                request.TargetLang
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetChallengesResponse(result.Challenges));
        })
        .WithName("GetChallenges")
        .WithTags("Challenges")
        .Produces<GetChallengesResponse>(200)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated Challenges with translation")
        .WithDescription("Returns paginated list of challenges filtered by ChallengeType. Title, Description, ChallengeType and Activities are translated if TargetLang is provided.");
    }
}
