using BuildingBlocks.CQRS;
using Post.Application.Features.Posts.Dtos;

namespace Post.Application.Features.Posts.Queries.GetPostById;

public record GetPostByIdQuery(Guid PostId) : IQuery<GetPostByIdResult>;

public record GetPostByIdResult(PostSummaryDto PostSummary);
