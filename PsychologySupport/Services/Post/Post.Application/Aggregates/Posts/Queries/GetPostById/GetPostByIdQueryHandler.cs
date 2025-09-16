using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;
using Post.Application.Aggregates.Posts.Dtos;
using Microsoft.EntityFrameworkCore;

namespace Post.Application.Aggregates.Posts.Queries.GetPostById;

internal sealed class GetPostByIdQueryHandler : IQueryHandler<GetPostByIdQuery, PostDto>
{
    private readonly IPostDbContext _context;

    public GetPostByIdQueryHandler(IPostDbContext context)
    {
        _context = context;
    }

    public async Task<PostDto> Handle(GetPostByIdQuery request, CancellationToken cancellationToken)
    {
        var postDto = await _context.Posts
            .Where(p => p.Id == request.PostId && !p.IsDeleted)
            .Select(p => new PostDto(
                p.Id,
                p.Content.Value,
                p.Content.Title,
                new AuthorDto(p.Author.AliasId, "Anonymous", ""),
                p.Visibility.ToString(),
                p.Moderation.Status.ToString(),
                p.Metrics.ReactionCount,
                p.Metrics.CommentCount,
                p.Metrics.ViewCount,
                p.CreatedAt,
                p.EditedAt,
                p.PublishedAt,
                p.Media.Select(m => m.MediaId.ToString()).ToList(),
                p.Categories.Select(c => c.CategoryTagId.ToString()).ToList()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (postDto is null)
        {
            throw new NotFoundException($"Post with ID {request.PostId} not found");
        }

        return postDto;
    }
}
