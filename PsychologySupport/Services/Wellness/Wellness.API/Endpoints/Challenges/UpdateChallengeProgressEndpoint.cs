using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.Application.Features.Challenges.Commands;
using Wellness.Domain.Enums;

namespace Wellness.API.Endpoints.Challenges
{
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
            app.MapPut("/v1/challenge-progress", async (
                [FromBody] UpdateChallengeProgressRequest request,
                ISender sender) =>
            {
                var command = request.Adapt<UpdateChallengeProgressCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateChallengeProgressResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateChallengeProgress")
            .WithTags("ChallengeProgress")
            .Produces<UpdateChallengeProgressResponse>(200)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Challenge Progress")
            .WithDescription("Update step progress in a challenge and recalculate overall progress.");
        }
    }
}
