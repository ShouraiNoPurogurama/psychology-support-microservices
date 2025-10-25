using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Wellness.API.Common.Authentication;
using Wellness.Application.Features.Challenges.Dtos;
using Wellness.Application.Features.Challenges.Queries;
using Wellness.Domain.Aggregates.Challenges.Enums;
using Wellness.Domain.Enums;

namespace Wellness.API.Endpoints.Challenges;

public record GetChallengeProgressRequest(
    ProcessStatus? ProcessStatus,
    ChallengeType? ChallengeType,
    int PageIndex = 1,
    int PageSize = 10,
    string? TargetLang = null
);

public record GetChallengeProgressResponse(PaginatedResult<ChallengeProgressDto> ChallengeProgresses);

public class GetChallengeProgressEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/challenge-progress", async (
            [AsParameters] GetChallengeProgressRequest request,
            ICurrentActorAccessor currentActor,
            ISender sender
        ) =>
        {
            var subjectRef = currentActor.GetRequiredSubjectRef();

            var query = new GetChallengeProgressQuery(
                SubjectRef: subjectRef,
                ProcessStatus: request.ProcessStatus,
                ChallengeType: request.ChallengeType,
                PaginationRequest: new PaginationRequest(request.PageIndex, request.PageSize),
                TargetLang: request.TargetLang
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
        .WithSummary("Get paginated ChallengeProgress with translation")
        .WithDescription("Returns the paginated challenge progress list for the current user, filtered by ProcessStatus and ChallengeType. Challenge and Activities are translated if TargetLang is provided.");
    }
}
