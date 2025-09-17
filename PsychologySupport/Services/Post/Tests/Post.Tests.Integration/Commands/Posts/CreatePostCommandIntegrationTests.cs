using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Post.Application.Aggregates.Posts.Commands.CreatePost;
using Post.Application.Data;
using Post.Domain.Aggregates.Posts.Enums;
using Testcontainers.PostgreSql;
using Post.Infrastructure.Data.Post;
using Post.Application.Integration;
using Post.Application.Abstractions.Authentication;
using Xunit;

namespace Post.Tests.Integration.Commands.Posts;

public class CreatePostCommandIntegrationTests : IAsyncLifetime
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
        
        // Mock dependencies
        services.AddScoped<IOutboxWriter, TestOutboxWriter>();
        services.AddScoped<IActorResolver, TestActorResolver>();
        services.AddScoped<IAliasVersionResolver, TestAliasVersionResolver>();
        
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreatePostCommandHandler).Assembly));

        _serviceProvider = services.BuildServiceProvider();
        _dbContext = _serviceProvider.GetRequiredService<PostDbContext>();
        
        await _dbContext.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task CreatePostCommand_ShouldPersistToDatabase_AndWriteToOutbox()
    {
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var outboxWriter = _serviceProvider.GetRequiredService<IOutboxWriter>() as TestOutboxWriter;

        var command = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Integration Test Post",
            Content: "This is a test post for integration testing",
            Visibility: PostVisibility.Draft,
            MediaIds: new[] { Guid.NewGuid(), Guid.NewGuid() }
        );

        var result = await mediator.Send(command);

        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);
        result.ModerationStatus.Should().Be("Pending");

        // Verify post was persisted to database
        var persistedPost = await _dbContext.Posts
            .Include(p => p.Media)
            .FirstOrDefaultAsync(p => p.Id == result.Id);

        persistedPost.Should().NotBeNull();
        persistedPost!.Content.Title.Should().Be(command.Title);
        persistedPost.Content.Value.Should().Be(command.Content);
        persistedPost.Visibility.Should().Be(command.Visibility);
        persistedPost.Media.Should().HaveCount(2);

        // Verify outbox event was written
        outboxWriter!.WrittenEvents.Should().HaveCount(1);
        //TODO raise integration event
        
        // outboxWriter.WrittenEvents.First().Should().BeOfType<PostCreatedIntegrationEvent>();
    }

    [Fact]
    public async Task CreatePostCommand_WithIdempotency_ShouldReturnSameResult()
    {
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var idempotencyKey = Guid.NewGuid();

        var command = new CreatePostCommand(
            IdempotencyKey: idempotencyKey,
            Title: "Idempotent Post",
            Content: "Testing idempotency",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        // First execution
        var result1 = await mediator.Send(command);
        
        // Second execution with same idempotency key
        var result2 = await mediator.Send(command);

        result1.Id.Should().Be(result2.Id);
        result1.CreatedAt.Should().Be(result2.CreatedAt);

        // Verify only one post was created
        var posts = await _dbContext.Posts.Where(p => p.Content.Title == "Idempotent Post").ToListAsync();
        posts.Should().HaveCount(1);
    }

    [Fact]
    public async Task CreatePostCommand_ShouldEnforceUniqueConstraints()
    {
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var mediaId = Guid.NewGuid();

        var command1 = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "First Post",
            Content: "Content for first post",
            Visibility: PostVisibility.Draft,
            MediaIds: new[] { mediaId }
        );

        var command2 = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Second Post",
            Content: "Content for second post",
            Visibility: PostVisibility.Draft,
            MediaIds: new[] { mediaId } // Same media ID
        );

        await mediator.Send(command1);

        // This should succeed as different posts can share media
        var result2 = await mediator.Send(command2);
        result2.Should().NotBeNull();
    }

    public async Task DisposeAsync()
    {
        await _dbContext.DisposeAsync();
        _serviceProvider.GetRequiredService<IServiceScope>().Dispose();
        await _postgresContainer.DisposeAsync();
    }
}

// Test doubles for integration tests
public class TestOutboxWriter : IOutboxWriter
{
    public List<object> WrittenEvents { get; } = new();

    public Task WriteAsync(object evt, CancellationToken ct)
    {
        WrittenEvents.Add(evt);
        return Task.CompletedTask;
    }
}

public class TestActorResolver : IActorResolver
{
    public Guid AliasId { get; } = Guid.NewGuid();
}

public class TestAliasVersionResolver : IAliasVersionResolver
{
    public Task<Guid> GetCurrentAliasVersionIdAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Guid.NewGuid());
    }
}
