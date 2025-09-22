Post Service Knowledge Context

Purpose
- Summarize architecture, key files, contracts, and behaviors in the Post microservice to onboard agents fast without re-reading sources.

Architecture & Projects
- Post.API: Carter endpoints for posts, comments, reactions, category tags, gifts; JWT auth; Swagger.
- Post.Application: CQRS handlers (MediatR), validators, mapping, integration events.
- Post.Domain: Aggregates and value objects (Post, Comment, Reaction, CategoryTag, Gift) + enums and domain events.
- Post.Infrastructure: EF Core DbContext, repositories/adapters, authentication accessors, DI wiring.
- Tests: Unit tests and integration tests projects exist under Services/Post/Tests.

Core Technologies
- Carter (Minimal APIs), MediatR (CQRS), FluentValidation, Mapster, EF Core (SQL Server), MassTransit (RabbitMQ), JWT.

API Layer (Carter)
- Endpoints folder: Services/Post/Post.API/Endpoints/* contains vertical slices:
  - Posts/: ApprovePostEndpoint.cs, AttachMediaToPostEndpoint.cs, and others (create/edit/publish are present in Application layer).
  - Comments/: CreateCommentEndpoint.cs, CreateReplyEndpoint.cs, GetCommentsEndpoint.cs.
  - Reactions/: endpoints for add/remove reactions.
  - CategoryTags/: AttachCategoryTagsEndpoint.cs (attach/detach patterns in Application).
  - Gifts/: AttachGiftEndpoint.cs.
- Endpoint pattern: sealed Request/Response records, ICarterModule.AddRoutes, RequireAuthorization(), and OpenAPI metadata.
- Auth: JWT; current alias and user resolved via accessors in Infrastructure.

Application Layer
- Commands/Queries by feature: Services/Post/Post.Application/Features/*
  - Posts/Commands: CreatePost, EditPost, Publish/Approve, AttachMediaToPost, etc.
  - Posts/Queries: e.g., fetch posts or details.
  - Comments/Commands & Queries: create, reply, list; includes DTOs and validators.
  - Reactions/Commands: add/remove reaction; Queries: fetch user reaction.
  - CategoryTags/Commands: attach/detach category tags to posts.
  - Gifts/Commands: attach gift to posts.
- Abstractions & Cross-cutting:
  - ICurrentActorAccessor for current alias/user; IAliasVersionAccessor for alias version lookup.
  - MediatR behaviors registered (ValidationBehavior, LoggingBehavior) from BuildingBlocks.
- Validation: Each Command/Query has a validator class with FluentValidation.
- Integration events: Published after successful state changes; see Messaging section.

Domain Layer
- Aggregates in Services/Post/Post.Domain/Aggregates/*
  - Posts: Post aggregate with content (PostContent VO), author (AuthorInfo VO), moderation (ModerationInfo), visibility and status enums.
  - Comments, Reactions, CategoryTag, Gift aggregates and related VOs.
- Enums: ModerationStatus, PostVisibility, PostStatus, PostMediaUpdateStatus.
- Domain events: PostCreated, etc., raised by aggregates for side effects.

Infrastructure Layer
- EF Core DbContext in Post.Infrastructure/Data/Post/PostDbContext.cs (per build hints). SQL Server provider (via connection strings).
- Authentication accessors in Post.Infrastructure/Authentication/
  - CurrentActorAccessor: exposes GetRequiredAliasId(), etc.
  - AliasVersionAccessor: resolves current alias version from JWT.
- DI in Post.Infrastructure/DependencyInjection.cs registers:
  - ICurrentActorAccessor, AliasVersionAccessor, Redis cache (BuildingBlocks.Extensions.AddRedisCache), MassTransit broker via Application.
- Persisters/adapters for repositories as needed.

Messaging (Shared BuildingBlocks)
- MassTransit: configured via BuildingBlocks.Messaging.MassTransit.Extensions.AddMessageBroker in Application DI.
- Integration events available in BuildingBlocks.Messaging/Events/IntegrationEvents/Posts/ include:
  - PostApprovedIntegrationEvent (PostId, AuthorAliasId, ModeratorAliasId, ApprovedAt).
  - PostCreatedWithMediaPendingIntegrationEvent (PostId, EntityType/MediaOwnerType, MediaIds).
  - PostMediaUpdatedIntegrationEvent.
  - PostCommentsLockStatusChangedIntegrationEvent.
  - PostCategoryTagsUpdatedIntegrationEvent.
  - ContentReportedIntegrationEvent.
- Post publishes relevant events; Feed, Notification, etc., may consume them.

Security & Auth
- JWT via MinimalJwtValidation.
- ICurrentActorAccessor and IAliasVersionAccessor used in handlers to enforce authorizations and resolve actor context.
- Endpoints require authorization unless explicitly public.

Database
- EF Core + SQL Server; migrations and DbContext in Post.Infrastructure.
- Relational modeling for posts, comments, reactions, category tags, gifts.

Testing
- Unit tests for handlers and validators (Post.Application.UnitTests).
- Integration tests (Post.Tests.Integration) for end-to-end flows such as CreatePostCommand, Publish/Approve, Reactions, Comments.

Quick Usage
- Build solution and run Post.API; sample flows:
  - Create Post: POST /v1/posts (idempotent) → returns post with moderation status.
  - Attach Media: POST /v1/posts/{postId}/media.
  - Approve Post: POST /v1/posts/{postId}/approve.
  - Comment: POST /v1/posts/{postId}/comments; Reply: POST /v1/comments/{commentId}/replies.
  - React: POST /v1/posts/{postId}/reactions; DELETE to remove.
- All protected endpoints require Authorization: Bearer <JWT> and use alias context.

Key File Map
- API: Post.API/Program.cs, DependencyInjection.cs, Endpoints/* by feature.
- Application: Post.Application/Features/* with Commands, Queries, DTOs, Validators, EventHandlers.
- Domain: Post.Domain/Aggregates/Posts/*, Enums, ValueObjects.
- Infrastructure: Post.Infrastructure/Authentication/*, Data/Post/PostDbContext.cs, DependencyInjection.cs.
- BuildingBlocks: Behaviors, CQRS, Messaging, Observability used across services.

Definition of Done (DoD) for new Post features
- Endpoint + Request/Response DTOs; CQRS + Validator; aggregate logic in Domain; EF Core persistence; integration events; RequireAuthorization; idempotency for write ops; Mapster mapping; Swagger docs; unit/integration tests.

Notes & Gaps
- Ensure event contract versions align with consumers.
- Keep idempotency for write endpoints using Idempotency-Key headers.
- Prefer AsNoTracking for read queries via read contexts.

