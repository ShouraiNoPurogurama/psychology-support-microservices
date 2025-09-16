using BuildingBlocks.Exceptions;
using Post.Application.Aggregates.Posts.Commands.EditPost;

namespace Post.UnitTests.Aggregates.Posts.Commands.EditPost
{
    public class EditPostCommandHandlerTests
    {
        private readonly Mock<IPostDbContext> _dbContextMock;
        private readonly Mock<IAliasVersionResolver> _aliasVersionResolverMock;
        private readonly Mock<IActorResolver> _actorResolverMock;
        private readonly Mock<IOutboxWriter> _outboxWriterMock;
        private readonly EditPostCommandHandler _handler;
        private readonly Guid _currentUserAliasId = Guid.NewGuid();
        private readonly Guid _postId = Guid.NewGuid();

        public EditPostCommandHandlerTests()
        {
            _dbContextMock = new Mock<IPostDbContext>();
            _actorResolverMock = new Mock<IActorResolver>();
            _aliasVersionResolverMock = new Mock<IAliasVersionResolver>();
            _outboxWriterMock = new Mock<IOutboxWriter>();

            // Common setup
            _actorResolverMock.Setup(x => x.AliasId).Returns(_currentUserAliasId);

            _handler = new EditPostCommandHandler(
                _dbContextMock.Object,
                _aliasVersionResolverMock.Object,
                _outboxWriterMock.Object);
        }

        [Fact]
        public async Task HandleEditPostCommandSuccessfully()
        {
            // Arrange
            var command = new EditPostCommand(
                AliasId: _currentUserAliasId,
                PostId: _postId,
                Content: "Updated post content",
                Title: "Updated Post Title",
                null,
                null
            );

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Original post content",
                "Original Post Title",
                Guid.NewGuid());

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_postId, result.PostId);

            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.Is<object>(o =>
                o.GetType().Name == "PostUpdatedIntegrationEvent"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleEditPostCommand_PostNotFound_ShouldThrowResourceNotFoundException()
        {
            // Arrange
            var command = new EditPostCommand(
                AliasId: _currentUserAliasId,
                PostId: _postId,
                Content: "Updated post content",
                Title: "Updated Post Title",
                null,
                null
            );

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Aggregates.Posts.Post)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleEditPostCommand_DifferentUser_ShouldThrowPostAuthorMismatchException()
        {
            // Arrange
            var command = new EditPostCommand(
                AliasId: _currentUserAliasId,
                PostId: _postId,
                Content: "Updated post content",
                Title: "Updated Post Title",
                null,
                null
            );

            var differentAuthorId = Guid.NewGuid();
            var post = Domain.Aggregates.Posts.Post.Create(
                differentAuthorId, // Different author
                "Original post content",
                "Original Post Title",
                Guid.NewGuid());

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act & Assert
            await Assert.ThrowsAsync<PostAuthorMismatchException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleEditPostCommand_DeletedPost_ShouldThrowDeletedPostActionException()
        {
            // Arrange
            var command = new EditPostCommand(
                AliasId: _currentUserAliasId,
                PostId: _postId,
                Content: "Updated post content",
                Title: "Updated Post Title",
                null,
                null
            );

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Original post content",
                "Original Post Title",
                Guid.NewGuid());

            post.Delete(_currentUserAliasId); // Mark post as deleted

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act & Assert
            await Assert.ThrowsAsync<DeletedPostActionException>(() =>
                _handler.Handle(command, CancellationToken.None));

            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleEditPostCommand_WithIdempotencyKey_ShouldNotDuplicateEdits()
        {
            // Arrange
            var command = new EditPostCommand(
                AliasId: _currentUserAliasId,
                PostId: _postId,
                Content: "Updated post content",
                Title: "Updated Post Title",
                null,
                null
            );

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Original post content",
                "Original Post Title",
                Guid.NewGuid());

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

            // Verify that no changes were saved on second call
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}