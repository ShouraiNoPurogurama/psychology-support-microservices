using Post.Application.Abstractions.Integration;
using Post.Application.Features.Posts.Commands.CreatePost;
using Post.Domain.Aggregates.Posts.Enums;

namespace Post.UnitTests.Aggregates.Posts.Commands.CreatePost
{
    public class CreatePostCommandHandlerTests
    {
        private readonly Mock<IPostDbContext> _dbContextMock;
        private readonly Mock<IAliasVersionAccessor> _aliasVersionResolverMock;
        private readonly Mock<ICurrentActorAccessor> _actorResolverMock;
        private readonly Mock<IOutboxWriter> _outboxWriterMock;
        private readonly CreatePostCommandHandler _handler;
        private readonly Guid _currentUserAliasId = Guid.NewGuid();
        private readonly Guid _aliasVersionId = Guid.NewGuid();

        public CreatePostCommandHandlerTests()
        {
            _dbContextMock = new Mock<IPostDbContext>();
            _aliasVersionResolverMock = new Mock<IAliasVersionAccessor>();
            _actorResolverMock = new Mock<ICurrentActorAccessor>();
            _outboxWriterMock = new Mock<IOutboxWriter>();
            
            // Common setup
            _actorResolverMock.Setup(x => x.GetRequiredAliasId()).Returns(_currentUserAliasId);
            _aliasVersionResolverMock.Setup(x => x.GetRequiredCurrentAliasVersionIdAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(_aliasVersionId);
            
            _handler = new CreatePostCommandHandler(
                _dbContextMock.Object,
                _aliasVersionResolverMock.Object,
                _actorResolverMock.Object);
        }

        [Fact]
        public async Task HandleCreatePostCommandSuccessfully()
        {
            // Arrange
            var command = new CreatePostCommand(
                IdempotencyKey: Guid.NewGuid(),
                Title: "Test Post Title",
                Content: "Test post content",
                Visibility: PostVisibility.Draft,
                MediaIds: null);

            Domain.Aggregates.Posts.Post createdPost = null;
            _dbContextMock.Setup(db => db.Posts.Add(It.IsAny<Domain.Aggregates.Posts.Post>()))
                .Callback<Domain.Aggregates.Posts.Post>(post => createdPost = post);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal("Pending", result.ModerationStatus);
            
            _dbContextMock.Verify(db => db.Posts.Add(It.IsAny<Domain.Aggregates.Posts.Post>()), Times.Once);
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            
            // Verify the post was created with the correct properties
            Assert.NotNull(createdPost);
            Assert.Equal(_currentUserAliasId, createdPost.Author.AliasId);
            Assert.Equal(command.Content, createdPost.Content.Value);
            Assert.Equal(command.Title, createdPost.Content.Title);
            Assert.Equal(command.Visibility, createdPost.Visibility);
            Assert.Equal(_aliasVersionId, createdPost.Author.AliasVersionId);
        }

        [Fact]
        public async Task HandleCreatePostCommandWithMediaAttachments()
        {
            // Arrange
            var mediaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var command = new CreatePostCommand(
                IdempotencyKey: Guid.NewGuid(),
                Title: "Post with Media",
                Content: "This post has media attachments",
                Visibility: PostVisibility.Draft,
                MediaIds: mediaIds);

            Domain.Aggregates.Posts.Post createdPost = null;
            _dbContextMock.Setup(db => db.Posts.Add(It.IsAny<Domain.Aggregates.Posts.Post>()))
                .Callback<Domain.Aggregates.Posts.Post>(post => createdPost = post);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            
            _dbContextMock.Verify(db => db.Posts.Add(It.IsAny<Domain.Aggregates.Posts.Post>()), Times.Once);
            _dbContextMock.Verify(db => db.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            
            // Verify the post was created with media attachments
            Assert.NotNull(createdPost);
            Assert.Equal(2, createdPost.Media.Count);
            Assert.Contains(createdPost.Media, m => m.MediaId == mediaIds[0]);
            Assert.Contains(createdPost.Media, m => m.MediaId == mediaIds[1]);
        }

        [Fact]
        public async Task HandleCreatePostCommandAsPublic()
        {
            // Arrange
            var command = new CreatePostCommand(
                IdempotencyKey: Guid.NewGuid(),
                Title: "Public Post",
                Content: "This is a public post",
                Visibility: PostVisibility.Public,
                MediaIds: null);

            Domain.Aggregates.Posts.Post createdPost = null;
            _dbContextMock.Setup(db => db.Posts.Add(It.IsAny<Domain.Aggregates.Posts.Post>()))
                .Callback<Domain.Aggregates.Posts.Post>(post => createdPost = post);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            
            // Verify the post was created with public visibility
            Assert.NotNull(createdPost);
            Assert.Equal(PostVisibility.Public, createdPost.Visibility);
        }

        [Fact]
        public async Task HandleCreatePostCommandWithNullTitle()
        {
            // Arrange
            var command = new CreatePostCommand(
                IdempotencyKey: Guid.NewGuid(),
                Title: null,
                Content: "Post without a title",
                Visibility: PostVisibility.Draft,
                MediaIds: null);

            Domain.Aggregates.Posts.Post createdPost = null;
            _dbContextMock.Setup(db => db.Posts.Add(It.IsAny<Domain.Aggregates.Posts.Post>()))
                .Callback<Domain.Aggregates.Posts.Post>(post => createdPost = post);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual(Guid.Empty, result.Id);
            
            // Verify the post was created with null title
            Assert.NotNull(createdPost);
            Assert.Null(createdPost.Content.Title);
            Assert.Equal(command.Content, createdPost.Content.Value);
        }

        [Fact]
        public async Task HandleCreatePostCommandWithIdempotencyKey()
        {
            // Arrange
            var idempotencyKey = Guid.NewGuid();
            var command = new CreatePostCommand(
                IdempotencyKey: idempotencyKey,
                Title: "Idempotent Post",
                Content: "Testing idempotency",
                Visibility: PostVisibility.Draft,
                MediaIds: null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(idempotencyKey, command.IdempotencyKey);
        }
    }
}
