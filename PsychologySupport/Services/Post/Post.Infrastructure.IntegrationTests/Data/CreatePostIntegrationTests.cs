                IdempotencyKey: idempotencyKey,
                Title: "Idempotent Post",
                Content: "Testing idempotency - first attempt",
                Visibility: PostVisibility.Draft,
                MediaIds: null);
                
            var command2 = new CreatePostCommand(
                IdempotencyKey: idempotencyKey, // Same idempotency key
                Title: "Idempotent Post - Second Attempt",
                Content: "Testing idempotency - second attempt",
                Visibility: PostVisibility.Draft,
                MediaIds: null);
            
            // Act
            var result1 = await handler.Handle(command1, CancellationToken.None);
            var result2 = await handler.Handle(command2, CancellationToken.None);
            
            // Assert
            Assert.Equal(result1.Id, result2.Id); // Should return same ID
            
            using (var context = new PostDbContext(_dbContextOptions))
            {
                var postCount = await context.Posts.CountAsync();
                Assert.Equal(1, postCount); // Should only have one post
                
                var savedPost = await context.Posts.FirstOrDefaultAsync(p => p.Id == result1.Id);
                Assert.NotNull(savedPost);
                // Should contain content from first request, not second
                Assert.Equal("Testing idempotency - first attempt", savedPost.Content.Value);
            }
        }
        
        [Fact]
        public async Task CreatePostWithOutbox_ShouldCreateOutboxMessage()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<CreatePostCommandHandler>();
            var outboxWriter = _serviceProvider.GetRequiredService<IOutboxWriter>() as Mock.MockOutboxWriter;
            
            var command = new CreatePostCommand(
                IdempotencyKey: Guid.NewGuid(),
                Title: "Outbox Test Post",
                Content: "Testing outbox integration events",
                Visibility: PostVisibility.Draft,
                MediaIds: null);
            
            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            
            // Assert
            Assert.NotNull(outboxWriter);
            Assert.True(outboxWriter.HasMessage("PostCreatedIntegrationEvent"));
            var message = outboxWriter.GetMessage("PostCreatedIntegrationEvent");
            Assert.NotNull(message);
            Assert.Contains(result.Id.ToString(), message);
        }
    }
    
    namespace Mock
    {
        public class MockAliasVersionResolver : IAliasVersionResolver
        {
            private Guid _currentAliasVersionId;
            
            public void SetCurrentAliasVersionId(Guid aliasVersionId)
            {
                _currentAliasVersionId = aliasVersionId;
            }
            
            public Task<Guid?> GetCurrentAliasVersionIdAsync(CancellationToken cancellationToken = default)
            {
                return Task.FromResult<Guid?>(_currentAliasVersionId);
            }
        }
        
        public class MockActorResolver : IActorResolver
        {
            private Guid _currentAliasId;
            
            public void SetCurrentAliasId(Guid aliasId)
            {
                _currentAliasId = aliasId;
            }
            
            public Guid AliasId => _currentAliasId;
            
            public Guid? UserId => Guid.NewGuid();
        }
        
        public class MockOutboxWriter : IOutboxWriter
        {
            private readonly Dictionary<string, string> _messages = new Dictionary<string, string>();
            
            public Task WriteAsync<T>(T message, CancellationToken cancellationToken = default) where T : class
            {
                var typeName = typeof(T).Name;
                _messages[typeName] = System.Text.Json.JsonSerializer.Serialize(message);
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Post.Application.Abstractions.Authentication;
using Post.Application.Aggregates.Posts.Commands.CreatePost;
using Post.Application.Data;
using Post.Application.Integration;
using Post.Domain.Aggregates.Posts.Enums;
using Post.Infrastructure.Data.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Post.Infrastructure.IntegrationTests.Data
{
    public class CreatePostIntegrationTests : IAsyncLifetime
    {
        private readonly Guid _currentUserAliasId = Guid.NewGuid();
        private readonly Guid _aliasVersionId = Guid.NewGuid();
        private readonly IContainer _postgresContainer;
        private IServiceProvider _serviceProvider;
        private DbContextOptions<PostDbContext> _dbContextOptions;
        
        public CreatePostIntegrationTests()
        {
            // Setup Postgres container
            _postgresContainer = new ContainerBuilder()
                .WithImage("postgres:latest")
                .WithEnvironment("POSTGRES_USER", "postgres")
                .WithEnvironment("POSTGRES_PASSWORD", "postgres")
                .WithEnvironment("POSTGRES_DB", "post_test_db")
                .WithPortBinding(5432, true)
                .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
                .Build();
        }

        public async Task InitializeAsync()
        {
            // Start container
            await _postgresContainer.StartAsync();
            
            // Get container port mapping
            var mappedPort = _postgresContainer.GetMappedPublicPort(5432);
            var connectionString = $"Server=localhost;Port={mappedPort};Database=post_test_db;User Id=postgres;Password=postgres;";
            
            // Setup DbContext options
            _dbContextOptions = new DbContextOptionsBuilder<PostDbContext>()
                .UseNpgsql(connectionString)
                .EnableSensitiveDataLogging()
                .Options;
            
            // Create and migrate database
            using (var context = new PostDbContext(_dbContextOptions))
            {
                await context.Database.MigrateAsync();
            }
            
            // Setup service provider
            var services = new ServiceCollection();
            
            // Mock services
            services.AddScoped<IPostDbContext>(provider => new PostDbContext(_dbContextOptions));
            services.AddScoped<IAliasVersionResolver>(provider => 
            {
                var mock = new Mock.MockAliasVersionResolver();
                mock.SetCurrentAliasVersionId(_aliasVersionId);
                return mock;
            });
            services.AddScoped<IActorResolver>(provider => 
            {
                var mock = new Mock.MockActorResolver();
                mock.SetCurrentAliasId(_currentUserAliasId);
                return mock;
            });
            services.AddScoped<IOutboxWriter, Mock.MockOutboxWriter>();
            services.AddScoped<CreatePostCommandHandler>();
            
            _serviceProvider = services.BuildServiceProvider();
        }
        
        public async Task DisposeAsync()
        {
            await _postgresContainer.StopAsync();
            await _postgresContainer.DisposeAsync();
        }
        
        [Fact]
        public async Task CreatePost_ShouldPersistPostToDatabase()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<CreatePostCommandHandler>();
            var command = new CreatePostCommand(
                IdempotencyKey: Guid.NewGuid(),
                Title: "Integration Test Post",
                Content: "This is a test post for integration testing",
                Visibility: PostVisibility.Draft,
                MediaIds: null);
            
            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            
            // Assert
            using (var context = new PostDbContext(_dbContextOptions))
            {
                var savedPost = await context.Posts
                    .FirstOrDefaultAsync(p => p.Id == result.Id);
                
                Assert.NotNull(savedPost);
                Assert.Equal(command.Title, savedPost.Content.Title);
                Assert.Equal(command.Content, savedPost.Content.Value);
                Assert.Equal(command.Visibility, savedPost.Visibility);
                Assert.Equal(_currentUserAliasId, savedPost.Author.AliasId);
                Assert.Equal(_aliasVersionId, savedPost.Author.AliasVersionId);
            }
        }
        
        [Fact]
        public async Task CreatePostWithMedia_ShouldPersistPostWithMediaToDatabase()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<CreatePostCommandHandler>();
            var mediaIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var command = new CreatePostCommand(
                IdempotencyKey: Guid.NewGuid(),
                Title: "Post with Media",
                Content: "This post has media attachments",
                Visibility: PostVisibility.Draft,
                MediaIds: mediaIds);
            
            // Act
            var result = await handler.Handle(command, CancellationToken.None);
            
            // Assert
            using (var context = new PostDbContext(_dbContextOptions))
            {
                var savedPost = await context.Posts
                    .Include(p => p.Media)
                    .FirstOrDefaultAsync(p => p.Id == result.Id);
                
                Assert.NotNull(savedPost);
                Assert.Equal(2, savedPost.Media.Count);
                Assert.Contains(savedPost.Media, m => m.MediaId == mediaIds[0]);
                Assert.Contains(savedPost.Media, m => m.MediaId == mediaIds[1]);
            }
        }
        
        [Fact]
        public async Task CreatePostWithSameIdempotencyKey_ShouldNotCreateDuplicatePosts()
        {
            // Arrange
            var handler = _serviceProvider.GetRequiredService<CreatePostCommandHandler>();
            var idempotencyKey = Guid.NewGuid();
            var command1 = new CreatePostCommand(
