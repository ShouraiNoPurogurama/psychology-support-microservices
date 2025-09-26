using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Application.Features.Challenges.Queries;
using Wellness.Domain.Aggregates.Challenges.Enums;

namespace Wellness.API.Endpoints.Challenges;

public record GetChallengesRequest(
    ChallengeType? ChallengeType,
    int PageIndex = 1,
    int PageSize = 10
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
                request.PageIndex,
                request.PageSize
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetChallengesResponse(result.Challenges));
        })
        .RequireAuthorization()
        .WithName("GetChallenges")
        .WithTags("Challenges")
        .Produces<GetChallengesResponse>(200)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated Challenges")
        .WithDescription("Returns paginated list of challenges filtered by ChallengeType, sorted by CreatedAt desc.");
    }
}
