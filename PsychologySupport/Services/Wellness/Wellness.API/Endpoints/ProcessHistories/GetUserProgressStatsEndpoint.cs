using Carter;
using MediatR;
using Wellness.API.Common.Authentication;
using Wellness.Application.Features.ProcessHistories.Dtos;
using Wellness.Application.Features.ProcessHistories.Queries;

namespace Wellness.API.Endpoints.ProcessHistories
{
    public record GetUserProgressStatsRequest(); // Không có param, vì dùng subjectRef từ token

    public record GetUserProgressStatsResponse(UserProgressStatsDto Stats);

    public class GetUserProgressStatsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/me/progress-stats", async (
                [AsParameters] GetUserProgressStatsRequest request,
                ICurrentActorAccessor currentActor,
                ISender sender
            ) =>
            {
                var subjectRef = currentActor.GetRequiredSubjectRef();

                var query = new GetUserProgressStatsQuery(subjectRef);

                var result = await sender.Send(query);

                return Results.Ok(new GetUserProgressStatsResponse(result.Stats));
            })
            .RequireAuthorization()
            .WithName("GetUserProgressStats")
            .WithTags("Progress")
            .Produces<GetUserProgressStatsResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .WithSummary("Get user progress statistics")
            .WithDescription("Returns overall progress statistics for the authenticated user including articles, challenges, and activities.");
        }
    }
}
