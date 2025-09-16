using BuildingBlocks.Exceptions;
using Post.Application.Aggregates.Posts.Commands.AttachMediaToPost;

namespace Post.UnitTests.Aggregates.Posts.Commands.AttachMediaToPost
{
    public class AttachMediaToPostCommandHandlerTests
    {
        private readonly Mock<IPostDbContext> _dbContextMock;
        private readonly Mock<IActorResolver> _actorResolverMock;
        private readonly Mock<IOutboxWriter> _outboxWriterMock;
        private readonly AttachMediaToPostCommandHandler _handler;
        private readonly Guid _currentUserAliasId = Guid.NewGuid();
        private readonly Guid _postId = Guid.NewGuid();
        private readonly Guid _mediaId = Guid.NewGuid();

        public AttachMediaToPostCommandHandlerTests()
        {
            _dbContextMock = new Mock<IPostDbContext>();
            _actorResolverMock = new Mock<IActorResolver>();
            _outboxWriterMock = new Mock<IOutboxWriter>();
            
            // Common setup
            _actorResolverMock.Setup(x => x.AliasId).Returns(_currentUserAliasId);
            
            _handler = new AttachMediaToPostCommandHandler(
                _dbContextMock.Object,
                _actorResolverMock.Object,
                _outboxWriterMock.Object);
        }

        [Fact]
        public async Task HandleAttachMediaToPostCommandSuccessfully()
        {
            // Arrange
            var command = new AttachMediaToPostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId,
                MediaId: _mediaId);

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid());

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_postId, result.PostId);
            Assert.Equal(_mediaId, result.MediaId);
            Assert.True(post.HasMedia);
            Assert.Single(post.Media);
            Assert.Equal(_mediaId, post.Media[0].MediaId);
            
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.Is<object>(o => 
                o.GetType().Name == "PostMediaAddedIntegrationEvent"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task HandleAttachMediaToPostCommand_PostNotFound_ShouldThrowResourceNotFoundException()
        {
            // Arrange
            var command = new AttachMediaToPostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId,
                MediaId: _mediaId);

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Aggregates.Posts.Post)null);

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleAttachMediaToPostCommand_DuplicateMedia_ShouldThrowInvalidPostDataException()
        {
            // Arrange
            var command = new AttachMediaToPostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId,
                MediaId: _mediaId);

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid());
            
            // Already add the media
            post.AddMedia(_mediaId);

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidPostDataException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleAttachMediaToPostCommand_MaxMediaExceeded_ShouldThrowInvalidPostDataException()
        {
            // Arrange
            var command = new AttachMediaToPostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId,
                MediaId: _mediaId);

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Test post content",
                "Test Post Title",
                Guid.NewGuid());
            
            // Add 10 media (maximum allowed)
            for (int i = 0; i < 10; i++)
            {
                post.AddMedia(Guid.NewGuid());
            }

            _dbContextMock.Setup(db => db.Posts.FindAsync(new object[] { _postId }, It.IsAny<CancellationToken>()))
                .ReturnsAsync(post);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidPostDataException>(() => 
                _handler.Handle(command, CancellationToken.None));
            
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task HandleAttachMediaToPostCommand_DeletedPost_ShouldThrowDeletedPostActionException()
        {
            // Arrange
            var command = new AttachMediaToPostCommand(
                IdempotencyKey: Guid.NewGuid(),
                PostId: _postId,
                MediaId: _mediaId);

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Test post content",
                "Test Post Title",
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
        public async Task HandleAttachMediaToPostCommand_WithIdempotencyKey_ShouldNotDuplicateAttachments()
        {
            // Arrange
            var idempotencyKey = Guid.NewGuid();
            var command = new AttachMediaToPostCommand(
                IdempotencyKey: idempotencyKey,
                PostId: _postId,
                MediaId: _mediaId);

            var post = Domain.Aggregates.Posts.Post.Create(
                _currentUserAliasId,
                "Test post content",
                "Test Post Title",
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
            Assert.Equal(result1.MediaId, result2.MediaId);
            
            // Verify that no changes were saved on second call
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            _outboxWriterMock.Verify(w => w.WriteAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
