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

public class CreatePostEndpointContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CreatePostEndpointContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace with in-memory database for contract tests
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<PostDbContext>));
                if (descriptor != null) services.Remove(descriptor);
                
                services.AddDbContext<PostDbContext>(options =>
                    options.UseInMemoryDatabase("ContractTestDb"));
            });
        });
        
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task CreatePostEndpoint_WithValidRequest_ReturnsCreatedWithCorrectSchema()
    {
        var request = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Contract Test Post",
            Content: "This is a contract test for create post endpoint",
            Visibility: PostVisibility.Draft,
            MediaIds: new[] { Guid.NewGuid() }
        );

        var response = await _client.PostAsJsonAsync("/api/posts", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();

        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<CreatePostResult>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        result.Should().NotBeNull();
        result!.Id.Should().NotBe(Guid.Empty);
        result.ModerationStatus.Should().Be("Pending");
        result.CreatedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreatePostEndpoint_WithInvalidContent_ReturnsBadRequestWithProblemDetails()
    {
        var request = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Invalid Post",
            Content: "", // Empty content should be invalid
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var response = await _client.PostAsJsonAsync("/api/posts", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Headers.Should().ContainKey("Content-Type");
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails.Should().NotBeNull();
        problemDetails!.Title.Should().Be("Validation failed");
        problemDetails.Errors.Should().ContainKey("Content");
    }

    [Fact]
    public async Task CreatePostEndpoint_WithMissingIdempotencyKey_ReturnsBadRequest()
    {
        var request = new CreatePostCommand(
            IdempotencyKey: Guid.Empty, // Invalid idempotency key
            Title: "Test Post",
            Content: "Valid content",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var response = await _client.PostAsJsonAsync("/api/posts", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails!.Errors.Should().ContainKey("IdempotencyKey");
    }

    [Fact]
    public async Task CreatePostEndpoint_WithIdempotencyHeader_ReturnsConsistentResults()
    {
        var idempotencyKey = Guid.NewGuid();
        var request = new CreatePostCommand(
            IdempotencyKey: idempotencyKey,
            Title: "Idempotent Test",
            Content: "Testing idempotency behavior",
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        _client.DefaultRequestHeaders.Add("Idempotency-Key", idempotencyKey.ToString());

        // First request
        var response1 = await _client.PostAsJsonAsync("/api/posts", request);
        var result1 = await response1.Content.ReadFromJsonAsync<CreatePostResult>();

        // Second request with same idempotency key
        var response2 = await _client.PostAsJsonAsync("/api/posts", request);
        var result2 = await response2.Content.ReadFromJsonAsync<CreatePostResult>();

        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.OK); // Should return existing result

        result1!.Id.Should().Be(result2!.Id);
        result1.CreatedAt.Should().Be(result2.CreatedAt);
    }

    [Fact]
    public async Task CreatePostEndpoint_WithExcessiveContentLength_ReturnsPayloadTooLarge()
    {
        var longContent = new string('a', 100001); // Exceeding reasonable content limit
        var request = new CreatePostCommand(
            IdempotencyKey: Guid.NewGuid(),
            Title: "Long Content Test",
            Content: longContent,
            Visibility: PostVisibility.Draft,
            MediaIds: null
        );

        var response = await _client.PostAsJsonAsync("/api/posts", request);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.RequestEntityTooLarge);
    }
}

public class ValidationProblemDetails
{
    public string Title { get; set; } = string.Empty;
    public Dictionary<string, string[]> Errors { get; set; } = new();
}
