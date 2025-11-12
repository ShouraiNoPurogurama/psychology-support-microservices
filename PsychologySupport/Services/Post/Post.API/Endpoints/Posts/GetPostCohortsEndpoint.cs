using Carter;
using MediatR;
using Post.Application.Features.Posts.Queries.GetPostCohort;

namespace Post.API.Endpoints.Posts;

public sealed class GetPostCohortsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/posts/dashboard/cohorts", async (
                DateOnly? startDate, int maxWeeks, ISender sender, CancellationToken ct) =>
            {
                var start = startDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-42));
                if (maxWeeks <= 0) maxWeeks = 7;

                var result = await sender.Send(new GetPostCohortsQuery(start, maxWeeks), ct);
                return Results.Ok(result);
            })
            .WithTags("Dashboard")
            .WithName("GetPostCohorts")
            .WithSummary("Weekly post cohort retention (Week0..WeekK)")
            .Produces<GetPostCohortsResult>(StatusCodes.Status200OK);
    }
}