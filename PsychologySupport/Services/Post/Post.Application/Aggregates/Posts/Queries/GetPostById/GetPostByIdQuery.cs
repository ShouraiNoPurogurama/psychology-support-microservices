using BuildingBlocks.CQRS;
using Post.Application.Aggregates.Posts.Dtos;

namespace Post.Application.Aggregates.Posts.Queries.GetPostById;

public record GetPostByIdQuery(Guid PostId) : IQuery<PostDto>;
