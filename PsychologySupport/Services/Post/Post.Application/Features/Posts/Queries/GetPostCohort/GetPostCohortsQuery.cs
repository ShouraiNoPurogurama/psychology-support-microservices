using BuildingBlocks.CQRS;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Posts.Queries.GetPostCohort;

public sealed record GetPostCohortsQuery(DateOnly StartDate, int MaxWeeks)
    : IQuery<GetPostCohortsResult>;

public sealed record GetPostCohortsResult(
    IReadOnlyList<PostCohortSeriesDto> Series
);