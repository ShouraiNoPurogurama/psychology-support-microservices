using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Data;

namespace Post.Application.Features.Posts.Commands.CreatePost;

public sealed class CreatePostCommandHandler : ICommandHandler<CreatePostCommand, CreatePostResult>
{
    private readonly IPostDbContext _context;
    private readonly IQueryDbContext _queryContext;
    private readonly IAliasVersionAccessor _aliasAccessor;
    private readonly ICurrentActorAccessor _currentActorAccessor;

    public CreatePostCommandHandler(
        IPostDbContext context,
        IAliasVersionAccessor aliasAccessor,
        ICurrentActorAccessor currentActorAccessor, IQueryDbContext queryContext)
    {
        _context = context;
        _aliasAccessor = aliasAccessor;
        _currentActorAccessor = currentActorAccessor;
        _queryContext = queryContext;
    }

    public async Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var aliasVersionId = await _aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(cancellationToken);
        var aliasId = _currentActorAccessor.GetRequiredAliasId();

        // Create post aggregate
        var post = Domain.Aggregates.Posts.Post.Create(
            _currentActorAccessor.GetRequiredAliasId(),
            request.Content,
            request.Title,
            aliasVersionId,
            request.Visibility
        );

        await AttachEmotionTagToPost(request, cancellationToken, aliasId, post);

        await AttachCategoryTagToPost(request, cancellationToken, post);

        // Add media if provided
        if (request.MediaIds?.Any() == true)
        {
            foreach (var mediaId in request.MediaIds)
            {
                post.AddMedia(mediaId);
            }
        }

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);

        return new CreatePostResult(
            post.Id,
            post.Moderation.Status.ToString(),
            post.CreatedAt
        );
    }

    private async Task AttachCategoryTagToPost(CreatePostCommand request, CancellationToken cancellationToken,
        Domain.Aggregates.Posts.Post post)
    {
        var tagExists = await _context.CategoryTags
            .AnyAsync(ct => ct.Id == request.CategoryTagId, cancellationToken);
        
        if(!tagExists)
            throw new NotFoundException("Tag bài viết không tồn tại.","TAG_NOT_FOUND");

        post.AddCategoryTag(request.CategoryTagId!.Value);
    }

    private async Task AttachEmotionTagToPost(CreatePostCommand request, CancellationToken cancellationToken, Guid aliasId,
        Domain.Aggregates.Posts.Post post)
    {
        var emotionQuery = _queryContext.EmotionTagReplicas
            .Where(e => e.Id == request.EmotionId);

        if (!emotionQuery.Any())
            throw new NotFoundException("Bạn chưa sở hữu tag cảm xúc này.");

        var userOwnsEmotionQuery = _queryContext.UserOwnedTagReplicas
            .Where(u => u.AliasId == aliasId);

        var resultQuery = from emotion in emotionQuery
            join userTag in userOwnsEmotionQuery
                on emotion.Id equals userTag.EmotionTagId
            select emotion;

        var ownedEmotion = await resultQuery.FirstOrDefaultAsync(cancellationToken)
                           ?? throw new NotFoundException("Tag cảm xúc không tồn tại.");

        post.AddEmotionTag(ownedEmotion.Id);
    }
}