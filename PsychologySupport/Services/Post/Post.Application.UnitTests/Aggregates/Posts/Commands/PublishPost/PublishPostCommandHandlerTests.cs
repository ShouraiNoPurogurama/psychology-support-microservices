using BuildingBlocks.Exceptions;
using Post.Application.Aggregates.Posts.Commands.PublishPost;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.UnitTests.Aggregates.Posts.Commands.PublishPost
{
    public class PublishPostCommandHandlerTests
    {
        private readonly Mock<IPostDbContext> _dbContextMock;
        private readonly Mock<IActorResolver> _actorResolverMock;
        private readonly Mock<IOutboxWriter> _outboxWriterMock;
        private readonly PublishPostCommandHandler _handler;
        private readonly Guid _currentUserAliasId = Guid.NewGuid();
        private readonly Guid _postId = Guid.NewGuid();

        public PublishPostCommandHandlerTests()
        {
            _dbContextMock = new Mock<IPostDbContext>();
            _actorResolverMock = new Mock<IActorResolver>();
            _outboxWriterMock = new Mock<IOutboxWriter>();
            
            // Common setup
            _actorResolverMock.Setup(x => x.AliasId).Returns(_currentUserAliasId);
            
            _handler = new PublishPostCommandHandler(
                _dbContextMock.Object,
                _actorResolverMock.Object,
                _outboxWriterMock.Object);
        }

        [Fact]
        public async Task HandlePublishPostCommandSuccessfully()
        {
            // Arrange
            var command = new PublishPostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId);

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid(),
                PostVisibility.Draft);
            
            // Approve the post first so it can be published
            post.Approve("1.0", Guid.NewGuid());

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_postId, result.PostId);
            Assert.Equal(PostVisibility.Public, result.Visibility);
            Assert.True(post.IsPublished);
            
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.Is<object>(o => 
                o.GetType().Name == "PostPublishedIntegrationEvent"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandlePublishPostCommand_PostNotFound_ShouldThrowResourceNotFoundException()
        {
            // Arrange
            var command = new PublishPostCommand(
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
        public async Task HandlePublishPostCommand_NotApproved_ShouldThrowInvalidPostDataException()
        {
            // Arrange
            var command = new PublishPostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId);

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid(),
                PostVisibility.Draft);
            
            // Post is not approved yet

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidPostDataException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandlePublishPostCommand_DeletedPost_ShouldThrowDeletedPostActionException()
        {
            // Arrange
            var command = new PublishPostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId);

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid());
            
            post.Approve("1.0", Guid.NewGuid());
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
        public async Task HandlePublishPostCommand_DifferentUser_ShouldThrowPostAuthorMismatchException()
        {
            // Arrange
            var command = new PublishPostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId);

            var differentAuthorId = Guid.NewGuid();
            var post = Domain.Aggregates.Posts.Post.Create(
                differentAuthorId, // Different author
                "Test post content",
                "Test Post Title",
                Guid.NewGuid());
            
            post.Approve("1.0", Guid.NewGuid());

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act & Assert
            await Assert.ThrowsAsync<PostAuthorMismatchException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandlePublishPostCommand_AlreadyPublic_ShouldReturnSuccessWithoutChanges()
        {
            // Arrange
            var command = new PublishPostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId);

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid(),
                PostVisibility.Draft);
            
            post.Approve("1.0", Guid.NewGuid());
            post.ChangeVisibility(PostVisibility.Public, _currentUserAliasId); // Already public

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_postId, result.PostId);
            Assert.Equal(PostVisibility.Public, result.Visibility);
            
            // Should not save changes or publish events since the post is already public
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandlePublishPostCommand_WithIdempotencyKey_ShouldNotDuplicatePublishing()
        {
            // Arrange
            var idempotencyKey = Guid.NewGuid();
            var command = new PublishPostCommand(
                IdempotencyKey: idempotencyKey,
                PostId: _postId);

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid(),
                PostVisibility.Draft);
            
            post.Approve("1.0", Guid.NewGuid());

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
            Assert.Equal(result1.Visibility, result2.Visibility);
            
            // Verify that no changes were saved on second call
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
