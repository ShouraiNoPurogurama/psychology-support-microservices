using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;
using Testcontainers.PostgreSql;
using Post.Infrastructure.Data.Post;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Authentication;
using Post.Application.Abstractions.Integration;
using Post.Application.Features.Posts.Commands.ApprovePost;
using Post.Application.Features.Posts.Commands.AttachMediaToPost;
using Post.Application.Features.Posts.Commands.CreatePost;
using Post.Application.Features.Posts.Commands.PublishPost;
using Xunit;

namespace Post.Tests.Integration.Commands.Posts;

public class Flow1PostLifecycleIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgresContainer = null!;
    private IServiceProvider _serviceProvider = null!;
    private PostDbContext _dbContext = null!;

    public async Task InitializeAsync()
    {
        _postgresContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithDatabase("post_test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        await _postgresContainer.StartAsync();

        var services = new ServiceCollection();
        
        services.AddDbContext<PostDbContext>(options =>
            options.UseNpgsql(_postgresContainer.GetConnectionString()));
        
        services.AddScoped<IPostDbContext>(provider => provider.GetRequiredService<PostDbContext>());
        
        services.AddScoped<IOutboxWriter, TestOutboxWriter>();
        services.AddScoped<ICurrentActorAccessor, TestCurrentActorAccessor>();
        services.AddScoped<IAliasVersionAccessor, TestAliasVersionAccessor>();
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AttachMediaToPostCommandHandler).Assembly));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<PostDbContext>();
        
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task AttachMediaToPostCommand_ShouldUpdateDatabase_AndWriteToOutbox()
    {
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var outboxWriter = _serviceProvider.GetRequiredService<IOutboxWriter>() as TestOutboxWriter;

        // Create a post first
        var post = Domain.Aggregates.Posts.Post.Create(
            Guid.NewGuid(),
            "Test content",
            "Test title",
            Guid.NewGuid(),
            PostVisibility.Draft);

        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync();
        outboxWriter!.WrittenEvents.Clear();

        var mediaId = Guid.NewGuid();
        var command = new AttachMediaToPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: post.Id,
            MediaId: mediaId,
            Position: 1
        );

        var result = await mediator.Send(command);

        result.Should().NotBeNull();
        result.PostId.Should().Be(post.Id);
        result.MediaId.Should().Be(mediaId);

        // Verify media was attached in database
        var updatedPost = await _dbContext.Posts
            .Include(p => p.Media)
            .FirstAsync(p => p.Id == post.Id);

        updatedPost.Media.Should().ContainSingle(m => m.MediaId == mediaId);

        // Verify outbox event
        outboxWriter.WrittenEvents.Should().HaveCount(1);
        outboxWriter.WrittenEvents.First().Should().BeOfType<PostMediaUpdatedIntegrationEvent>();
    }

    [Fact]
    public async Task PublishPostCommand_ShouldUpdateVisibility_AndWriteToOutbox()
    {
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var outboxWriter = _serviceProvider.GetRequiredService<IOutboxWriter>() as TestOutboxWriter;

        // Create a draft post
        var post = Domain.Aggregates.Posts.Post.Create(
            Guid.NewGuid(),
            "Test content",
            "Test title",
            Guid.NewGuid(),
            PostVisibility.Draft);

        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync();
        outboxWriter!.WrittenEvents.Clear();

        var command = new PublishPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: post.Id
        );

        var result = await mediator.Send(command);

        result.Should().NotBeNull();
        result.PostId.Should().Be(post.Id);
        result.Visibility.Should().Be(PostVisibility.Public);

        // Verify post visibility updated in database
        var updatedPost = await _dbContext.Posts.FindAsync(post.Id);
        updatedPost!.Visibility.Should().Be(PostVisibility.Public);

        // Verify outbox event
        outboxWriter.WrittenEvents.Should().HaveCount(1);
        outboxWriter.WrittenEvents.First().Should().BeOfType<PostPublishedIntegrationEvent>();
    }

    [Fact]
    public async Task ApprovePostCommand_ShouldUpdateModerationStatus_AndWriteToOutbox()
    {
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var outboxWriter = _serviceProvider.GetRequiredService<IOutboxWriter>() as TestOutboxWriter;

        // Create a published post
        var post = Domain.Aggregates.Posts.Post.Create(
            Guid.NewGuid(),
            "Test content",
            "Test title",
            Guid.NewGuid(),
            PostVisibility.Public);

        _dbContext.Posts.Add(post);
        await _dbContext.SaveChangesAsync();
        outboxWriter!.WrittenEvents.Clear();

        var command = new ApprovePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: post.Id
        );

        var result = await mediator.Send(command);

        result.Should().NotBeNull();
        result.PostId.Should().Be(post.Id);
        result.ModerationStatus.Should().Be(ModerationStatus.Approved);

        // Verify moderation status updated in database
        var updatedPost = await _dbContext.Posts.FindAsync(post.Id);
        updatedPost!.Moderation.Status.Should().Be(ModerationStatus.Approved);

        // Verify outbox event
        outboxWriter.WrittenEvents.Should().HaveCount(1);
        outboxWriter.WrittenEvents.First().Should().BeOfType<PostApprovedIntegrationEvent>();
    }

    [Fact]
    public async Task Flow1CompleteLifecycle_ShouldMaintainConsistency_AndWriteAllOutboxEvents()
    {
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var outboxWriter = _serviceProvider.GetRequiredService<IOutboxWriter>() as TestOutboxWriter;

        // Step 1: Create post
        var createCommand = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Flow 1 Complete Test",
            Content: "Testing complete Flow 1 lifecycle",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var createResult = await mediator.Send(createCommand);
        var postId = createResult.Id;

        // Step 2: Attach media
        var mediaId = Guid.NewGuid();
        var attachMediaCommand = new AttachMediaToPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: postId,
            MediaId: mediaId,
            Position: null
        );

        await mediator.Send(attachMediaCommand);

        // Step 3: Publish post
        var publishCommand = new PublishPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: postId
        );

        await mediator.Send(publishCommand);

        // Step 4: Approve post
        var approveCommand = new ApprovePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: postId
        );

        await mediator.Send(approveCommand);

        // Verify final state in database
        var finalPost = await _dbContext.Posts
            .Include(p => p.Media)
            .FirstAsync(p => p.Id == postId);

        finalPost.Visibility.Should().Be(PostVisibility.Public);
        finalPost.Moderation.Status.Should().Be(ModerationStatus.Approved);
        finalPost.Media.Should().ContainSingle(m => m.MediaId == mediaId);

        // Verify all outbox events were written
        var eventTypes = outboxWriter!.WrittenEvents.Select(e => e.GetType()).ToList();
        // eventTypes.Should().Contain(typeof(PostCreatedIntegrationEvent));
        eventTypes.Should().Contain(typeof(PostMediaUpdatedIntegrationEvent));
        eventTypes.Should().Contain(typeof(PostPublishedIntegrationEvent));
        eventTypes.Should().Contain(typeof(PostApprovedIntegrationEvent));
    }

    [Fact]
    public async Task AttachMediaToNonExistentPost_ShouldThrowNotFoundException()
    {
        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        var command = new AttachMediaToPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: Guid.NewGuid(),
            MediaId: Guid.NewGuid(),
            Position: null
        );

        await FluentActions.Invoking(() => mediator.Send(command))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Post not found*");
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        _serviceProvider.GetRequiredService<IServiceScope>().Dispose();
        await _postgresContainer.DisposeAsync();
    }
}
