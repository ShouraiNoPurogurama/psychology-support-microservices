using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common.Authentication;
using Wellness.Application.Features.Challenges.Commands;
using Wellness.Domain.Enums;

namespace Wellness.API.Endpoints.Challenges;

public record UpdateChallengeProgressRequest(
    Guid ChallengeProgressId,
    Guid StepId,
    ProcessStatus StepStatus,
    Guid? PostMoodId
);

public record UpdateChallengeProgressResponse(
    Guid ChallengeProgressId,
    ProcessStatus ProcessStatus,
    int ProgressPercent
);

public class UpdateChallengeProgressEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/v1/me/challenge-progress", async (
            [FromBody] UpdateChallengeProgressRequest request,
            ICurrentActorAccessor currentActor,
            ISender sender
        ) =>
        {
            var subjectRef = currentActor.GetRequiredSubjectRef();

            var command = new UpdateChallengeProgressCommand(
                SubjectRef: subjectRef,
                ChallengeProgressId: request.ChallengeProgressId,
                StepId: request.StepId,
                StepStatus: request.StepStatus,
                PostMoodId: request.PostMoodId
            );

            var result = await sender.Send(command);

            var response = new UpdateChallengeProgressResponse(
                result.ChallengeProgressId,
                result.ProcessStatus,
                result.ProgressPercent
            );

            return Results.Ok(response);
        })
        .RequireAuthorization()
        .WithName("UpdateChallengeProgress")
        .WithTags("ChallengeProgress")
        .Produces<UpdateChallengeProgressResponse>(200)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Update Challenge Progress")
        .WithDescription("Update step progress in a challenge for the current user and recalculate overall progress.");
    }
}
