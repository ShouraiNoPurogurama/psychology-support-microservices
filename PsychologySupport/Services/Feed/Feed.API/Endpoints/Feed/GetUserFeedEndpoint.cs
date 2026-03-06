using Carter;
using Feed.API.Extensions;
using Feed.Application.Features.UserFeed.Queries.GetFeed;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Feed.API.Endpoints.Feed;

public sealed record GetUserFeedRequest(int? Limit, string? Cursor);
public sealed record GetUserFeedResponse(IReadOnlyList<UserFeedItemDto> Items, string? NextCursor, bool HasMore, int TotalCount);

public class GetUserFeedEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/feed", async ([FromQuery] int? limit, [FromQuery] string? cursor, ISender sender, HttpContext http, CancellationToken ct) =>
            {
                var aliasId = http.User.GetAliasId();
                var pageSize = Math.Clamp(limit ?? 20, 1, 100);

                var query = new GetFeedQuery(aliasId, PageIndex: 0, PageSize: pageSize, Cursor: cursor);
                var result = await sender.Send(query, ct);

                var response = new GetUserFeedResponse(result.Items, result.NextCursor, result.HasMore, result.TotalCount);
                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithTags("Feed")
            .Produces<GetUserFeedResponse>(StatusCodes.Status200OK)
            .WithName("GetUserFeed");
    }
}

