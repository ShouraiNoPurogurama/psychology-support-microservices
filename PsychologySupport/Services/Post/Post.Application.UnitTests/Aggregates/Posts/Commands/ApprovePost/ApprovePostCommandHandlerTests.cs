using BuildingBlocks.Exceptions;
using Post.Application.Aggregates.Posts.Commands.ApprovePost;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.UnitTests.Aggregates.Posts.Commands.ApprovePost
{
    public class ApprovePostCommandHandlerTests
    {
        private readonly Mock<IPostDbContext> _dbContextMock;
        private readonly Mock<IActorResolver> _actorResolverMock;
        private readonly Mock<IOutboxWriter> _outboxWriterMock;
        private readonly ApprovePostCommandHandler _handler;
        private readonly Guid _moderatorAliasId = Guid.NewGuid();
        private readonly Guid _postId = Guid.NewGuid();

        public ApprovePostCommandHandlerTests()
        {
            _dbContextMock = new Mock<IPostDbContext>();
            _actorResolverMock = new Mock<IActorResolver>();
            _outboxWriterMock = new Mock<IOutboxWriter>();
            
            // Common setup
            _actorResolverMock.Setup(x => x.AliasId).Returns(_moderatorAliasId);
            
            _handler = new ApprovePostCommandHandler(
                _dbContextMock.Object,
                _actorResolverMock.Object,
                _outboxWriterMock.Object);
        }

        [Fact]
        public async Task HandleApprovePostCommandSuccessfully()
        {
            // Arrange
            var command = new ApprovePostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId);

            var authorAliasId = Guid.NewGuid();
            var post = Domain.Aggregates.Posts.Post.Create(
                authorAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid(),
                PostVisibility.Draft);

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_postId, result.PostId);
            Assert.Equal(ModerationStatus.Approved, result.ModerationStatus);
            Assert.True(post.CanBePublished);
            
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.Is<object>(o => 
                o.GetType().Name == "PostApprovedIntegrationEvent"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleApprovePostCommand_PostNotFound_ShouldThrowResourceNotFoundException()
        {
            // Arrange
            var command = new ApprovePostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId);

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Aggregates.Posts.Post)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleApprovePostCommand_AlreadyApproved_ShouldReturnSuccessWithoutChanges()
        {
            // Arrange
            var command = new ApprovePostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId);

            var authorAliasId = Guid.NewGuid();
            var post = Domain.Aggregates.Posts.Post.Create(
                authorAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid(),
                PostVisibility.Draft);
            
            // Already approve the post
            post.Approve("0.9", Guid.NewGuid());

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_postId, result.PostId);
            Assert.Equal(ModerationStatus.Approved, result.ModerationStatus);
            
            // Should still save changes to update the policy version
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.Is<object>(o => 
                o.GetType().Name == "PostApprovedIntegrationEvent"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleApprovePostCommand_WithIdempotencyKey_ShouldNotDuplicateApproval()
        {
            // Arrange
            var idempotencyKey = Guid.NewGuid();
            var command = new ApprovePostCommand(
                IdempotencyKey: idempotencyKey,
                PostId: _postId);

            var authorAliasId = Guid.NewGuid();
            var post = Domain.Aggregates.Posts.Post.Create(
                authorAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid(),
                PostVisibility.Draft);

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // First call
            var result1 = await _handler.Handle(command, CancellationToken.None);
            
            // Reset mock to verify second call doesn't save changes
            _dbContextMock.Invocations.Clear();
            _outboxWriterMock.Invocations.Clear();
            
            // Second call with same idempotency key
            var result2 = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.Equal(result1.PostId, result2.PostId);
            Assert.Equal(result1.ModerationStatus, result2.ModerationStatus);
            
            // Verify that no changes were saved on second call
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
