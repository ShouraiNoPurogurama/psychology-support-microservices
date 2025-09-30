using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Application.Features.Challenges.Queries;
using Wellness.Domain.Aggregates.Challenges.Enums;
using Wellness.Domain.Enums;

namespace Wellness.API.Endpoints.Challenges;

public record GetChallengeProgressRequest(
    Guid SubjectRef,
    ProcessStatus? ProcessStatus,
    ChallengeType? ChallengeType,
    int PageIndex = 1,
    int PageSize = 10
);

public record GetChallengeProgressResponse(PaginatedResult<ChallengeProgressDto> ChallengeProgresses);

public class GetChallengeProgressEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/challenge-progress", async (
            [AsParameters] GetChallengeProgressRequest request,
            ISender sender, HttpContext httpContext) =>
        {

            // Authorization check
            if (!AuthorizationHelpers.CanView(request.SubjectRef, httpContext.User))
                throw new ForbiddenException();

            var query = new GetChallengeProgressQuery(
                request.SubjectRef,
                request.ProcessStatus,
                request.ChallengeType,
                request.PageIndex,
                request.PageSize
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetChallengeProgressResponse(result.ChallengeProgresses));
        })
        .RequireAuthorization()
        .WithName("GetChallengeProgress")
        .WithTags("ChallengeProgress")
        .Produces<GetChallengeProgressResponse>(200)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated ChallengeProgress")
        .WithDescription("Returns paginated challenge progress list filtered by SubjectRef, ProcessStatus, and ChallengeType. Sorted by StartDate desc and ChallengeType asc.");
    }
}
