using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Post.Application.Abstractions.Authentication;
using Post.Application.Aggregates.Posts.Commands.AttachMediaToPost;
using Post.Application.Data;
using Post.Application.Integration;
using BuildingBlocks.Exceptions;

namespace Post.UnitTests.Aggregates.Posts.Commands.AttachMediaToPost;

public class AttachMediaToPostCommandHandlerTests
{
    private readonly Mock<IPostDbContext> _dbContextMock;
    private readonly Mock<IActorResolver> _actorResolverMock;
    private readonly Mock<IOutboxWriter> _outboxWriterMock;
    private readonly Mock<DbSet<Domain.Aggregates.Posts.Post>> _postsDbSetMock;
    private readonly AttachMediaToPostCommandHandler _handler;
    private readonly Guid _currentUserAliasId = Guid.NewGuid();

    public AttachMediaToPostCommandHandlerTests()
    {
        _dbContextMock = new Mock<IPostDbContext>();
        _actorResolverMock = new Mock<IActorResolver>();
        _outboxWriterMock = new Mock<IOutboxWriter>();
        _postsDbSetMock = new Mock<DbSet<Domain.Aggregates.Posts.Post>>();
        
        _actorResolverMock.Setup(x => x.AliasId).Returns(_currentUserAliasId);
        _dbContextMock.Setup(x => x.Posts).Returns(_postsDbSetMock.Object);
        
        _handler = new AttachMediaToPostCommandHandler(
            _dbContextMock.Object,
            _actorResolverMock.Object,
            _outboxWriterMock.Object);
    }

    [Fact]
    public async Task HandleAttachMediaToPostCommandSuccessfully()
    {
        var postId = Guid.NewGuid();
        var mediaId = Guid.NewGuid();
        var position = 1;
        
        var post = Domain.Aggregates.Posts.Post.Create(
            _currentUserAliasId,
            "Test content",
            "Test title",
            Guid.NewGuid(),
            Domain.Aggregates.Posts.Enums.PostVisibility.Draft);

        var command = new AttachMediaToPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: postId,
            MediaId: mediaId,
            Position: position);

        _postsDbSetMock.SetupAsyncEnumerable(new[] { post });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.PostId.Should().Be(postId);
        result.MediaId.Should().Be(mediaId);
        
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _outboxWriterMock.Verify(x => x.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAttachMediaToPostCommandWithNonExistentPost()
    {
        var command = new AttachMediaToPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: Guid.NewGuid(),
            MediaId: Guid.NewGuid(),
            Position: null);

        _postsDbSetMock.SetupAsyncEnumerable(Array.Empty<Domain.Aggregates.Posts.Post>());

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("Post not found or has been deleted.");
    }

    [Fact]
    public async Task HandleAttachMediaToPostCommandWithoutPosition()
    {
        var postId = Guid.NewGuid();
        var mediaId = Guid.NewGuid();
        
        var post = Domain.Aggregates.Posts.Post.Create(
            _currentUserAliasId,
            "Test content",
            "Test title",
            Guid.NewGuid(),
            Domain.Aggregates.Posts.Enums.PostVisibility.Draft);

        var command = new AttachMediaToPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: postId,
            MediaId: mediaId,
            Position: null);

        _postsDbSetMock.SetupAsyncEnumerable(new[] { post });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.PostId.Should().Be(postId);
        result.MediaId.Should().Be(mediaId);
        
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _outboxWriterMock.Verify(x => x.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAttachMediaToPostCommandWithIdempotencyKey()
    {
        var idempotencyKey = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var mediaId = Guid.NewGuid();
        
        var post = Domain.Aggregates.Posts.Post.Create(
            _currentUserAliasId,
            "Test content",
            "Test title",
            Guid.NewGuid(),
            Domain.Aggregates.Posts.Enums.PostVisibility.Draft);

        var command = new AttachMediaToPostCommand(
            IdempotencyKey: idempotencyKey,
            PostId: postId,
            MediaId: mediaId,
            Position: 0);

        _postsDbSetMock.SetupAsyncEnumerable(new[] { post });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        command.IdempotencyKey.Should().Be(idempotencyKey);
    }
}
