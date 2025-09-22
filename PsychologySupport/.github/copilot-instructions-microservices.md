# Psychology Support Platform - Microservices Architecture Instructions

## Overview
This document defines the architectural patterns, conventions, and coding standards for the Psychology Support Platform microservices, with focus on **Post**, **Feed**, **Media**, and **Alias** services. All code must follow Domain-Driven Design (DDD) principles with Clean Architecture and CQRS patterns.

---

## 🏗️ Architecture Patterns

### Microservice Structure
Each microservice follows the Clean Architecture pattern with these layers:

```
ServiceName/
├── ServiceName.API/           # Presentation Layer (Carter endpoints)
├── ServiceName.Application/   # Application Layer (CQRS handlers)
├── ServiceName.Domain/        # Domain Layer (Aggregates, VOs, Events)
├── ServiceName.Infrastructure/ # Infrastructure Layer (EF Core, External APIs)
└── Tests/                     # Unit & Integration Tests
```

### Core Technologies
- **API Framework**: Carter (Minimal APIs with routing)
- **CQRS/Mediator**: MediatR
- **Validation**: FluentValidation
- **Mapping**: Mapster
- **ORM**: Entity Framework Core
- **Messaging**: MassTransit (RabbitMQ)
- **Authentication**: JWT with Alias-based authorization

---

## 📋 Coding Conventions

### 1. Endpoint Structure (API Layer)

```csharp
/**
 * Endpoint class following Carter pattern
 * - Sealed record for Request/Response DTOs
 * - ICarterModule implementation
 * - Proper HTTP status codes and OpenAPI documentation
 */
public sealed record CreatePostRequest(
    string? Title,
    string Content,
    PostVisibility Visibility,
    IEnumerable<Guid>? MediaIds = null,
    Guid? CategoryTagId = null
);

public sealed record CreatePostResponse(
    Guid Id,
    string ModerationStatus,
    DateTimeOffset CreatedAt
);

public class CreatePostEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/posts", async (
                CreatePostRequest request,
                [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
                ISender sender,
                CancellationToken ct) =>
            {
                // Idempotency validation
                if (requestKey is null || requestKey == Guid.Empty)
                    throw new BadRequestException("Missing Idempotency-Key header");

                // Map to Command/Query
                var command = new CreatePostCommand(/*...*/);
                var result = await sender.Send(command, ct);
                
                // Map to Response DTO
                var response = result.Adapt<CreatePostResponse>();
                return Results.Created($"/v1/posts/{response.Id}", response);
            })
            .RequireAuthorization()
            .WithTags("Posts")
            .WithOpenApiDocumentation();
    }
}
```

### 2. Command/Query Structure (Application Layer)

```csharp
/**
 * Command record inheriting from IdempotentCommand for write operations
 * - Immutable record with validation-friendly properties
 * - Separate Result record for handler return values
 */
public record CreatePostCommand(
    Guid IdempotencyKey,
    string? Title,
    string Content,
    PostVisibility Visibility,
    IEnumerable<Guid>? MediaIds = null
) : IdempotentCommand<CreatePostResult>(IdempotencyKey);

public record CreatePostResult(
    Guid Id,
    string ModerationStatus,
    DateTimeOffset CreatedAt
);
```

### 3. Command/Query Handlers

```csharp
/**
 * Handler implementing ICommandHandler or IQueryHandler
 * - Constructor injection for dependencies
 * - Proper DbContext usage (IPostDbContext for writes, IQueryDbContext for reads)
 * - Integration event publishing via MassTransit
 * - Domain aggregate usage for business logic
 */
public sealed class CreatePostCommandHandler(
    IPostDbContext context,
    IAliasVersionAccessor aliasAccessor,
    ICurrentActorAccessor currentActorAccessor,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<CreatePostCommand, CreatePostResult>
{
    public async Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        // Get current user context
        var aliasVersionId = await aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(cancellationToken);
        var aliasId = currentActorAccessor.GetRequiredAliasId();

        // Create domain aggregate
        var post = Domain.Aggregates.Posts.Post.Create(
            aliasId,
            request.Content,
            request.Title,
            aliasVersionId,
            request.Visibility
        );

        // Business logic through aggregate methods
        if (request.MediaIds?.Any() == true)
        {
            foreach (var mediaId in request.MediaIds)
                post.AddMedia(mediaId);
        }

        // Persist changes
        context.Posts.Add(post);
        await context.SaveChangesAsync(cancellationToken);

        // Publish integration event
        var integrationEvent = new PostCreatedIntegrationEvent(post.Id, request.MediaIds);
        await publishEndpoint.Publish(integrationEvent, cancellationToken);

        return new CreatePostResult(post.Id, post.Moderation.Status.ToString(), post.CreatedAt);
    }
}
```

### 4. Validation (FluentValidation)

```csharp
/**
 * Validator class for each Command/Query
 * - Inherits from AbstractValidator<T>
 * - Validates all input parameters with business rules
 * - Custom validation methods for complex rules
 */
public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.IdempotencyKey)
            .NotEmpty()
            .WithMessage("Idempotency key is required");

        RuleFor(x => x.Content)
            .NotEmpty()
            .WithMessage("Content is required")
            .MaximumLength(10000)
            .WithMessage("Content cannot exceed 10000 characters");

        RuleFor(x => x.MediaIds)
            .Must(HaveUniqueMediaIds)
            .WithMessage("Media IDs must be unique")
            .When(x => x.MediaIds != null);
    }

    private static bool HaveUniqueMediaIds(IEnumerable<Guid>? mediaIds)
    {
        if (mediaIds == null) return true;
        var mediaIdList = mediaIds.ToList();
        return mediaIdList.Count == mediaIdList.Distinct().Count();
    }
}
```

### 5. Domain Aggregates

```csharp
/**
 * Domain aggregate following DDD patterns
 * - Inherits from AggregateRoot<TId>
 * - Private setters, public factory methods
 * - Value Objects for complex properties
 * - Domain events for side effects
 * - Business logic encapsulation
 */
public sealed class Post : AggregateRoot<Guid>, ISoftDeletable
{
    // Value Objects
    public PostContent Content { get; private set; } = null!;
    public AuthorInfo Author { get; private set; } = null!;
    public ModerationInfo Moderation { get; private set; } = null!;

    // Properties
    public PostVisibility Visibility { get; private set; }
    public PostStatus Status { get; private set; }

    // Collections (private backing fields)
    private readonly List<PostMedia> _media = new();
    public IReadOnlyList<PostMedia> Media => _media.AsReadOnly();

    // Factory method
    public static Post Create(
        string aliasId,
        string content,
        string? title,
        Guid aliasVersionId,
        PostVisibility visibility)
    {
        var post = new Post
        {
            Id = Guid.NewGuid(),
            Content = PostContent.Create(content, title),
            Author = AuthorInfo.Create(aliasId, aliasVersionId),
            Visibility = visibility,
            Status = PostStatus.Creating
        };

        // Raise domain event
        post.RaiseDomainEvent(new PostCreatedDomainEvent(post.Id));
        return post;
    }

    // Business logic methods
    public void AddMedia(Guid mediaId)
    {
        if (_media.Any(m => m.MediaId == mediaId))
            throw new DomainException("Media already attached to post");

        _media.Add(new PostMedia(Id, mediaId));
    }
}
```

---

## 🔧 Implementation Guidelines

### Database Context Patterns

```csharp
/**
 * Separate contexts for Command and Query operations
 * - IPostDbContext: Write operations with change tracking
 * - IQueryDbContext: Read operations with AsNoTracking()
 */

// Command Context (Writes)
public interface IPostDbContext
{
    DbSet<Post> Posts { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// Query Context (Reads)
public interface IQueryDbContext
{
    IQueryable<Post> Posts { get; }
}

// Usage in handlers
public class GetPostsQueryHandler : IQueryHandler<GetPostsQuery, GetPostsResult>
{
    public async Task<GetPostsResult> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        var posts = await queryContext.Posts
            .AsNoTracking()  // Always use for reads
            .Where(p => !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(request.PageNumber * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new GetPostsResult(posts.Adapt<List<PostDto>>());
    }
}
```

### Pagination Pattern

```csharp
/**
 * Standardized pagination for all list operations
 * - PaginationRequest for input parameters
 * - PaginatedResult<T> for response wrapper
 */
public record GetPostsQuery(
    int PageIndex = 0,
    int PageSize = 20,
    string? SearchTerm = null,
    PostVisibility? Visibility = null
) : IQuery<GetPostsResult>;

public record GetPostsResult(
    PaginatedResult<PostDto> Result
);

[PaginatedResult already exists in BuildingBlocks/Pagination]
public record PaginatedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize,
    bool HasNextPage,
    bool HasPreviousPage
);
```

### Integration Events Pattern

```csharp
/**
 * Integration events for cross-service communication
 * - Versioned event contracts in BuildingBlocks.Messaging
 * - Published via MassTransit after successful database operations
 */
namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Posts;

public record PostCreatedWithMediaPendingIntegrationEvent(
    Guid PostId,
    string EntityType,
    IEnumerable<Guid>? MediaIds
) : IntegrationEvent;

// Usage in handler
await publishEndpoint.Publish(
    new PostCreatedWithMediaPendingIntegrationEvent(post.Id, "Post", request.MediaIds),
    cancellationToken);
```

### Authentication & Authorization

```csharp
/**
 * Alias-based authentication system
 * - IAliasVersionAccessor: Get current alias version
 * - ICurrentActorAccessor: Get current user/alias context
 * - RequireAuthorization() on all protected endpoints
 */
public class CreatePostCommandHandler
{
    public async Task<CreatePostResult> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        // Get current user context
        var aliasVersionId = await aliasAccessor.GetRequiredCurrentAliasVersionIdAsync(cancellationToken);
        var aliasId = currentActorAccessor.GetRequiredAliasId();
        
        // Use in business logic...
    }
}
```

---

/**
* MICROSERVICE-SPECIFIC PATTERNS DOCUMENTATION
* ============================================
*
* This section defines the architectural patterns and implementation details
* for the four core microservices in the Psychology Support Platform.
* Each service follows either Clean Architecture (4-layer) or Vertical Slice patterns.
  */

## 📂 Microservice-Specific Patterns

/**
* POST SERVICE - Clean Architecture Pattern
* ========================================
*
* Architecture: 4-Layer Clean Architecture
* - Post.API (Presentation)
* - Post.Application (Application Logic)
* - Post.Domain (Business Rules)
* - Post.Infrastructure (Data Access)
*
* Primary Database: SQL Server with Entity Framework Core
* Messaging: RabbitMQ via MassTransit for integration events
  */
### Post Service
- **Aggregates**: Post, Comment, Reaction, CategoryTag, Gift
- **Key Features**: Content moderation, media attachment, visibility controls
- **Integration Events**: PostCreated, PostUpdated, PostDeleted, PostModerated

/**
* FEED SERVICE - Vertical Slice Pattern
* =====================================
*
* Architecture: Single Feed.API project with vertical slices
* - Features organized by business capability (e.g., Timeline, Recommendations)
* - Each feature contains: Endpoint, Command/Query, Handler, Validator in same folder
*
* Primary Database: Apache Cassandra (optimized for time-series data)
* Search Engine: Elasticsearch integration for advanced post searching
* Caching: Redis for frequently accessed timelines
*
* Design Rationale:
* - High read throughput requirements (social media feeds)
* - Denormalized data structure suits Cassandra's strengths
* - Vertical slices reduce cognitive load for feed-specific features
    */
### Feed Service
- **Purpose**: Aggregated view of posts from followed users
- **Pattern**: Read-only projections from Post events
- **Key Features**: Timeline generation, filtering, caching

/**
* MEDIA SERVICE - Clean Architecture Pattern
* =========================================
*
* Architecture: 4-Layer Clean Architecture
* - Media.API (File upload endpoints)
* - Media.Application (Processing workflows)
* - Media.Domain (Media metadata rules)
* - Media.Infrastructure (Cloud storage, CDN)
*
* Storage: Cloud blob storage with CDN integration
* Processing: Background jobs for image/video processing
  */
### Media Service
- **Aggregates**: Media, MediaMetadata
- **Key Features**: File upload, processing, CDN integration
- **Integration Events**: MediaUploaded, MediaProcessed, MediaDeleted

/**
* ALIAS SERVICE - Clean Architecture Pattern
* ==========================================
*
* Architecture: 4-Layer Clean Architecture
* - Alias.API (Identity management endpoints)
* - Alias.Application (User workflow orchestration)
* - Alias.Domain (Identity and versioning rules)
* - Alias.Infrastructure (User data persistence)
*
* Key Responsibility: Manages user identities with version control
* - Supports pseudonymous posting through alias versions
* - Maintains user privacy while enabling accountability
    */
### Alias Service
- **Aggregates**: Alias, AliasVersion
- **Key Features**: User identity management, version control
- **Integration Events**: AliasCreated, AliasVersionCreated, AliasUpdated

/**
* ARCHITECTURE PATTERN SELECTION CRITERIA
* =======================================
*
* Clean Architecture (4-Layer) - Used for:
* - Complex business domain logic (Post, Media, Alias)
* - Multiple integration points
* - Rich domain models with invariants
* - Long-term maintainability requirements
*
* Vertical Slice - Used for:
* - Query-heavy services (Feed)
* - Rapid feature development
* - Minimal cross-feature dependencies
* - Performance-critical read operations
*
* DATABASE SELECTION RATIONALE
* ============================
*
* SQL Server (Post, Media, Alias):
* - ACID compliance for critical business data
* - Complex relational queries
* - Strong consistency requirements
*
* Cassandra (Feed):
* - Optimized for time-series data (user timelines)
* - Horizontal scalability for high read volumes
* - Eventual consistency acceptable for feed data
*
* Elasticsearch (Feed - Search):
* - Full-text search capabilities
* - Faceted search and filtering
* - Real-time indexing of post content
*
* INTEGRATION PATTERNS
* ===================
*
* Event-Driven Communication:
* - Post Service publishes PostCreated → Feed Service updates timelines
* - Media Service publishes MediaProcessed → Post Service updates attachments
* - Alias Service publishes AliasVersionCreated → All services update references
*
* Saga Pattern:
* - Post creation with media attachment (Post ↔ Media coordination)
* - User registration workflow (Auth → Alias → Profile coordination)
    */


---

## ✅ Definition of Done Checklist

For every new feature implementation:

- [ ] **Endpoint**: Carter module with Request/Response records
- [ ] **Command/Query**: Proper CQRS pattern with separate Result record
- [ ] **Handler**: ICommandHandler/IQueryHandler with correct DbContext usage
- [ ] **Validation**: FluentValidation class with comprehensive rules
- [ ] **Domain Logic**: Business rules in aggregates, not handlers
- [ ] **Mapping**: Mapster configurations for DTO transformations
- [ ] **Events**: Integration events for cross-service communication
- [ ] **Authorization**: RequireAuthorization() with proper access control
- [ ] **Idempotency**: Idempotency-Key header for write operations
- [ ] **Pagination**: PaginatedResult for list operations
- [ ] **Error Handling**: Proper exception types and status codes
- [ ] **Documentation**: OpenAPI/Swagger documentation
- [ ] **Tests**: Unit tests for handlers and validators
- [ ] **Integration Tests**: End-to-end API tests

---

## 🚨 Anti-Patterns to Avoid

1. **No Anonymous Types**: Always use proper Request/Response records
2. **No Direct Entity Returns**: Always map to DTOs before returning
3. **No Business Logic in Endpoints**: Keep endpoints thin, logic in handlers/aggregates
4. **No Cross-Context Joins**: Never join IPostDbContext with IQueryDbContext
5. **No Missing Validation**: Every Command/Query must have a validator
6. **No Forgotten AsNoTracking()**: Always use for read operations
7. **No Missing Integration Events**: Publish events for state changes affecting other services
8. **No Hardcoded Values**: Use enums and constants instead of magic strings
9. **No Missing Idempotency**: All write operations must support idempotency
10. **No Unauthorized Endpoints**: All endpoints must have proper authorization

---

## 🔄 Development Workflow

1. **Plan**: Identify Command/Query, aggregates, DTOs, events needed
2. **Domain**: Implement/update domain aggregates and value objects
3. **Application**: Create Command/Query, Handler, Validator
4. **API**: Implement Carter endpoint with proper DTOs
5. **Integration**: Add integration events and event handlers
6. **Mapping**: Update Mapster configurations
7. **Tests**: Write unit and integration tests
8. **Documentation**: Update OpenAPI specifications
9. **Validation**: Run tests and check for errors
10. **Review**: Ensure all Definition of Done criteria are met

This architecture ensures consistency, maintainability, and scalability across all microservices in the Psychology Support Platform.
