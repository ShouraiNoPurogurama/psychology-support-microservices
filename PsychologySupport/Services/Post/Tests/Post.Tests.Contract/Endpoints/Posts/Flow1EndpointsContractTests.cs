using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Post.API;
using Post.Application.Aggregates.Posts.Commands.CreatePost;
using Post.Application.Aggregates.Posts.Commands.AttachMediaToPost;
using Post.Application.Aggregates.Posts.Commands.PublishPost;
using Post.Application.Aggregates.Posts.Commands.ApprovePost;
using Post.Domain.Aggregates.Posts.Enums;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Post.Infrastructure.Data.Post;

namespace Post.Tests.Contract.Endpoints.Posts;

public class Flow1EndpointsContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public Flow1EndpointsContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PostDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                
                services.AddDbContext<PostDbContext>(options =>
                    options.UseInMemoryDatabase($"ContractTestDb_{Guid.NewGuid()}"));
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AttachMediaEndpoint_WithValidRequest_ReturnsOkWithCorrectSchema()
    {
        // First create a post
        var createRequest = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Test Post for Media",
            Content: "Content for media attachment test",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var createResponse = await _client.PostAsJsonAsync("/api/posts", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreatePostResult>();

        // Now attach media
        var attachRequest = new AttachMediaToPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: createResult!.Id,
            MediaId: Guid.NewGuid(),
            Position: 1
        );

        var response = await _client.PostAsJsonAsync($"/api/posts/{createResult.Id}/media", attachRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<AttachMediaToPostResult>();
        result.Should().NotBeNull();
        result!.PostId.Should().Be(createResult.Id);
        result.MediaId.Should().Be(attachRequest.MediaId);
        result.AttachedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task AttachMediaEndpoint_WithNonExistentPost_ReturnsNotFound()
    {
        var attachRequest = new AttachMediaToPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: Guid.NewGuid(),
            MediaId: Guid.NewGuid(),
            Position: null
        );

        var response = await _client.PostAsJsonAsync($"/api/posts/{attachRequest.PostId}/media", attachRequest);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task PublishPostEndpoint_WithValidRequest_ReturnsOkWithCorrectSchema()
    {
        // Create a draft post first
        var createRequest = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Draft Post",
            Content: "Content to be published",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var createResponse = await _client.PostAsJsonAsync("/api/posts", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreatePostResult>();

        // Publish the post
        var publishRequest = new PublishPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: createResult!.Id
        );

        var response = await _client.PostAsJsonAsync($"/api/posts/{createResult.Id}/publish", publishRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<PublishPostResult>();
        result.Should().NotBeNull();
        result!.PostId.Should().Be(createResult.Id);
        result.Visibility.Should().Be(PostVisibility.Public);
        result.PublishedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ApprovePostEndpoint_WithValidRequest_ReturnsOkWithCorrectSchema()
    {
        // Create and publish a post first
        var createRequest = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Post for Approval",
            Content: "Content for moderation approval test",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var createResponse = await _client.PostAsJsonAsync("/api/posts", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreatePostResult>();

        var publishRequest = new PublishPostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: createResult!.Id
        );

        await _client.PostAsJsonAsync($"/api/posts/{createResult.Id}/publish", publishRequest);

        // Approve the post
        var approveRequest = new ApprovePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: createResult.Id
        );

        // Add moderator authorization header
        _client.DefaultRequestHeaders.Add("X-User-Role", "Moderator");

        var response = await _client.PostAsJsonAsync($"/api/posts/{createResult.Id}/approve", approveRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<ApprovePostResult>();
        result.Should().NotBeNull();
        result!.PostId.Should().Be(createResult.Id);
        result.ModerationStatus.Should().Be(ModerationStatus.Approved);
        result.ApprovedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task ApprovePostEndpoint_WithoutModeratorRole_ReturnsForbidden()
    {
        var createRequest = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Post for Forbidden Test",
            Content: "Testing authorization",
            Visibility: PostVisibility.Public,
            MediaIds: null
        );

        var createResponse = await _client.PostAsJsonAsync("/api/posts", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreatePostResult>();

        var approveRequest = new ApprovePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            PostId: createResult!.Id
        );

        // No moderator role header
        var response = await _client.PostAsJsonAsync($"/api/posts/{createResult.Id}/approve", approveRequest);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Flow1CompleteEndpointSequence_ShouldMaintainConsistentState()
    {
        var idempotencyKeys = new
        {
            Create = Guid.NewGuid(),
            AttachMedia = Guid.NewGuid(),
            Publish = Guid.NewGuid(),
            Approve = Guid.NewGuid()
        };

        // Step 1: Create post
        var createRequest = new CreatePostCommand(
            IdempotencyKey: idempotencyKeys.Create,
            Title: "Complete Flow Test",
            Content: "Testing complete Flow 1 via API endpoints",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        _client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKeys.Create.ToString());
        var createResponse = await _client.PostAsJsonAsync("/api/posts", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<CreatePostResult>();

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // Step 2: Attach media
        var attachRequest = new AttachMediaToPostCommand(
            IdempotencyKey: idempotencyKeys.AttachMedia,
            PostId: createResult!.Id,
            MediaId: Guid.NewGuid(),
            Position: null
        );

        _client.DefaultRequestHeaders.Remove("Idempotency-Key");
        _client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKeys.AttachMedia.ToString());
        var attachResponse = await _client.PostAsJsonAsync($"/api/posts/{createResult.Id}/media", attachRequest);

        attachResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: Publish post
        var publishRequest = new PublishPostCommand(
            IdempotencyKey: idempotencyKeys.Publish,
            PostId: createResult.Id
        );

        _client.DefaultRequestHeaders.Remove("Idempotency-Key");
        _client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKeys.Publish.ToString());
        var publishResponse = await _client.PostAsJsonAsync($"/api/posts/{createResult.Id}/publish", publishRequest);

        publishResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 4: Approve post
        var approveRequest = new ApprovePostCommand(
            IdempotencyKey: idempotencyKeys.Approve,
            PostId: createResult.Id
        );

        _client.DefaultRequestHeaders.Remove("Idempotency-Key");
        _client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKeys.Approve.ToString());
        _client.DefaultRequestHeaders.Add("X-User-Role", "Moderator");
        var approveResponse = await _client.PostAsJsonAsync($"/api/posts/{createResult.Id}/approve", approveRequest);

        approveResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify final state via GET endpoint
        var getResponse = await _client.GetAsync($"/api/posts/{createResult.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var postDto = await getResponse.Content.ReadFromJsonAsync<PostDto>();
        postDto.Should().NotBeNull();
        postDto!.Id.Should().Be(createResult.Id);
        postDto.Visibility.Should().Be("Public");
        postDto.ModerationStatus.Should().Be("Approved");
        postDto.Media.Should().HaveCount(1);
    }
}

public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public string ModerationStatus { get; set; } = string.Empty;
    public List<MediaDto> Media { get; set; } = new();
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}

public class MediaDto
{
    public Guid MediaId { get; set; }
    public int Position { get; set; }
    public string? AltText { get; set; }
}
