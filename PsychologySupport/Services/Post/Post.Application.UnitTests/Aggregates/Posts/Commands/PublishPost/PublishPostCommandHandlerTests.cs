using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Integration;
using Post.Application.Features.Posts.Commands.PublishPost;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.UnitTests.Aggregates.Posts.Commands.PublishPost;

public class PublishPostCommandHandlerTests
{
    private readonly Mock<IPostDbContext> _dbContextMock;
    private readonly Mock<ICurrentActorAccessor> _actorResolverMock;
    private readonly Mock<IOutboxWriter> _outboxWriterMock;
    private readonly Mock<DbSet<Domain.Aggregates.Posts.Post>> _postsDbSetMock;
    private readonly PublishPostCommandHandler _handler;
    private readonly Guid _currentUserAliasId = Guid.NewGuid();

    public PublishPostCommandHandlerTests()
    {
        _dbContextMock = new Mock<IPostDbContext>();
        _actorResolverMock = new Mock<ICurrentActorAccessor>();
        _outboxWriterMock = new Mock<IOutboxWriter>();
        _postsDbSetMock = new Mock<DbSet<Domain.Aggregates.Posts.Post>>();
        
        _actorResolverMock.Setup(x => x.GetRequiredAliasId()).Returns(_currentUserAliasId);
        _dbContextMock.Setup(x => x.Posts).Returns(_postsDbSetMock.Object);
        
        _handler = new PublishPostCommandHandler(
            _dbContextMock.Object,
            _actorResolverMock.Object,
            _outboxWriterMock.Object);
    }

    [Fact]
    public async Task HandlePublishPostCommandSuccessfully()
    {
        var postId = Guid.NewGuid();
        var post = Domain.Aggregates.Posts.Post.Create(
            _currentUserAliasId,
            "Test content",
            "Test title",
            Guid.NewGuid(),
            PostVisibility.Draft);

        var command = new PublishPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: postId);

        _postsDbSetMock.SetupAsyncEnumerable(new[] { post });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.PostId.Should().Be(postId);
        result.Visibility.Should().Be(PostVisibility.Public);
        
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _outboxWriterMock.Verify(x => x.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandlePublishPostCommandWithNonExistentPost()
    {
        var command = new PublishPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: Guid.NewGuid());

        _postsDbSetMock.SetupAsyncEnumerable(Array.Empty<Domain.Aggregates.Posts.Post>());

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Post not found*");
    }

    [Fact]
    public async Task HandlePublishPostCommandWithDeletedPost()
    {
        var postId = Guid.NewGuid();
        var post = Domain.Aggregates.Posts.Post.Create(
            _currentUserAliasId,
            "Test content",
            "Test title",
            Guid.NewGuid(),
            PostVisibility.Draft);
        
        // Simulate soft delete
        post.SoftDelete(_currentUserAliasId);

        var command = new PublishPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: postId);

        _postsDbSetMock.SetupAsyncEnumerable(new[] { post });

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandlePublishPostCommandWithIdempotencyKey()
    {
        var idempotencyKey = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var post = Domain.Aggregates.Posts.Post.Create(
            _currentUserAliasId,
            "Test content",
            "Test title",
            Guid.NewGuid(),
            PostVisibility.Draft);

        var command = new PublishPostCommand(
            IdempotencyKey: idempotencyKey,
            PostId: postId);

        _postsDbSetMock.SetupAsyncEnumerable(new[] { post });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        command.IdempotencyKey.Should().Be(idempotencyKey);
    }
}
