using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Exceptions;
using Post.Application.Abstractions.Integration;
using Post.Application.Features.Posts.Commands.ApprovePost;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.UnitTests.Aggregates.Posts.Commands.ApprovePost;

public class ApprovePostCommandHandlerTests
{
    private readonly Mock<IPostDbContext> _dbContextMock;
    private readonly Mock<ICurrentActorAccessor> _actorResolverMock;
    private readonly Mock<IOutboxWriter> _outboxWriterMock;
    private readonly Mock<DbSet<Domain.Aggregates.Posts.Post>> _postsDbSetMock;
    private readonly ApprovePostCommandHandler _handler;
    private readonly Guid _moderatorAliasId = Guid.NewGuid();

    public ApprovePostCommandHandlerTests()
    {
        _dbContextMock = new Mock<IPostDbContext>();
        _actorResolverMock = new Mock<ICurrentActorAccessor>();
        _outboxWriterMock = new Mock<IOutboxWriter>();
        _postsDbSetMock = new Mock<DbSet<Domain.Aggregates.Posts.Post>>();
        
        _actorResolverMock.Setup(x => x.GetRequiredAliasId()).Returns(_moderatorAliasId);
        _dbContextMock.Setup(x => x.Posts).Returns(_postsDbSetMock.Object);
        
        _handler = new ApprovePostCommandHandler(
            _dbContextMock.Object,
            _actorResolverMock.Object,
            _outboxWriterMock.Object);
    }

    [Fact]
    public async Task HandleApprovePostCommandSuccessfully()
    {
        var postId = Guid.NewGuid();
        var authorAliasId = Guid.NewGuid();
        var post = Domain.Aggregates.Posts.Post.Create(
            authorAliasId,
            "Test content",
            "Test title",
            Guid.NewGuid(),
            PostVisibility.Public);

        var command = new ApprovePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: postId);

        _postsDbSetMock.SetupAsyncEnumerable(new[] { post });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.PostId.Should().Be(postId);
        result.ModerationStatus.Should().Be(ModerationStatus.Approved);
        
        _dbContextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _outboxWriterMock.Verify(x => x.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleApprovePostCommandWithNonExistentPost()
    {
        var command = new ApprovePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: Guid.NewGuid());

        _postsDbSetMock.SetupAsyncEnumerable(Array.Empty<Domain.Aggregates.Posts.Post>());

        await FluentActions.Invoking(() => _handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>()
            .WithMessage("*Post not found*");
    }

    [Fact]
    public async Task HandleApprovePostCommandWithoutReason()
    {
        var postId = Guid.NewGuid();
        var authorAliasId = Guid.NewGuid();
        var post = Domain.Aggregates.Posts.Post.Create(
            authorAliasId,
            "Test content",
            "Test title",
            Guid.NewGuid(),
            PostVisibility.Public);

        var command = new ApprovePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: postId);

        _postsDbSetMock.SetupAsyncEnumerable(new[] { post });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.PostId.Should().Be(postId);
        result.ModerationStatus.Should().Be(ModerationStatus.Approved);
    }

    [Fact]
    public async Task HandleApprovePostCommandWithIdempotencyKey()
    {
        var idempotencyKey = Guid.NewGuid();
        var postId = Guid.NewGuid();
        var authorAliasId = Guid.NewGuid();
        var post = Domain.Aggregates.Posts.Post.Create(
            authorAliasId,
            "Test content",
            "Test title",
            Guid.NewGuid(),
            PostVisibility.Public);

        var command = new ApprovePostCommand(
            IdempotencyKey: idempotencyKey,
            PostId: postId);

        _postsDbSetMock.SetupAsyncEnumerable(new[] { post });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        command.IdempotencyKey.Should().Be(idempotencyKey);
    }

    [Fact]
    public async Task HandleApprovePostCommandForAlreadyApprovedPost()
    {
        var postId = Guid.NewGuid();
        var authorAliasId = Guid.NewGuid();
        var post = Domain.Aggregates.Posts.Post.Create(
            authorAliasId,
            "Test content",
            "Test title",
            Guid.NewGuid(),
            PostVisibility.Public);
        
        // Simulate already approved post
        post.Approve("policy_v1",_moderatorAliasId);

        var command = new ApprovePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: postId);

        _postsDbSetMock.SetupAsyncEnumerable(new[] { post });

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.ModerationStatus.Should().Be(ModerationStatus.Approved);
    }
}
