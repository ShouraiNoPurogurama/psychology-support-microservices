using BuildingBlocks.Constants;
using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.Application.Features.Posts.Commands.CreatePost;

public sealed class CreatePostCommandHandler(
    IPostDbContext context,
    IAliasVersionAccessor aliasAccessor,
    ICurrentActorAccessor currentActorAccessor,
    IQueryDbContext queryContext,
    IPublishEndpoint publishEndpoint,
    IOutboxWriter outboxWriter,
    IFollowerCountProvider followerCountProvider)
    : ICommandHandler<CreatePostCommand, CreatePostResult>
{

    public async Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        var aliasVersionId = await aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(cancellationToken);
        var aliasId = currentActorAccessor.GetRequiredAliasId();

        var post = Domain.Aggregates.Posts.Post.Create(
            currentActorAccessor.GetRequiredAliasId(),
            request.Content,
            request.Title,
            aliasVersionId,
            request.Visibility
        );

        // Moderation will be handled by AIModeration service via integration events
        // Post status remains in Creating/Pending state until moderation completes

        await AttachEmotionTagToPost(request, cancellationToken, aliasId, post);

        await AttachCategoryTagToPost(request, cancellationToken, post);

        var hasMedia = request.Medias.Any();
        if (hasMedia)
        {
            foreach (var media in request.Medias)
            {
                post.AddMedia(media.MediaId, media.Url);
            }
        }

        //Bypass moderation, auto-publish if possible
        // if (post.CanBePublished)
        // {
        //     post.ChangeVisibility(PostVisibility.Public, editorAliasId: post.Author.AliasId);
        //     await outboxWriter.WriteAsync(new PostApprovedIntegrationEvent(post.Id, aliasId, SystemActors.SystemUUID, DateTimeOffset.UtcNow), cancellationToken);
        // }

        context.Posts.Add(post);

        await outboxWriter.WriteAsync(new PostCreatedIntegrationEvent(post.Id, aliasId, post.CreatedAt, post.Content.Title, post.Content.Value), cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        // Publish media processing event ONLY when media is present
        if (hasMedia)
        {
            var integrationEvent = new PostCreatedWithMediaPendingIntegrationEvent(post.Id, "Post", request.Medias!.Select(x => x.MediaId));
            await publishEndpoint.Publish(integrationEvent, cancellationToken);
        }

        return new CreatePostResult(
            post.Id,
            post.Moderation.Status.ToString(),
            post.CreatedAt
        );
    }

    private async Task AttachCategoryTagToPost(CreatePostCommand request, CancellationToken cancellationToken,
        Domain.Aggregates.Posts.Post post)
    {
        if(request.CategoryTagId is null) return;

        var tagExists = await context.CategoryTags
            .AnyAsync(ct => ct.Id == request.CategoryTagId, cancellationToken);

        if (!tagExists)
            throw new NotFoundException("Tag bài viết không tồn tại.", "TAG_NOT_FOUND");

        post.AddCategoryTag(request.CategoryTagId!.Value);
    }

    private async Task AttachEmotionTagToPost(CreatePostCommand request, CancellationToken cancellationToken, Guid aliasId,
        Domain.Aggregates.Posts.Post post)
    {
        if (request.EmotionId is null) return;

        var emotionQuery = queryContext.EmotionTagReplicas
            .Where(e => e.Id == request.EmotionId);

        if (!emotionQuery.Any())
            throw new NotFoundException("Bạn chưa sở hữu tag cảm xúc này.");

        var userOwnsEmotionQuery = queryContext.UserOwnedTagReplicas
            .Where(u => u.AliasId == aliasId);

        // var resultQuery = from emotion in emotionQuery
        //     join userTag in userOwnsEmotionQuery
        //         on emotion.Id equals userTag.EmotionTagId
        //     select emotion;

        var resultQuery = emotionQuery;

        var ownedEmotion = await resultQuery.FirstOrDefaultAsync(cancellationToken)
                           ?? throw new NotFoundException("Tag cảm xúc không tồn tại.");

        post.AddEmotionTag(ownedEmotion.Id);
    }
}
