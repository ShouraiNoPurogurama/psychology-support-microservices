Alias Service Knowledge Context

Purpose
- Summarize architecture, key files, contracts, and behaviors in the Alias microservice to onboard agents fast without re-reading sources.

Architecture & Projects
- Alias.API: All features implemented here (vertical slice). Carter endpoints, JWT auth, Swagger, DI, data access.
- BuildingBlocks.*: Shared CQRS, Messaging (MassTransit), Behaviors, Observability, Redis connection helper.

Core Technologies
- Carter (Minimal APIs), MediatR (CQRS), FluentValidation (expected for inputs), Mapster (mapping), MassTransit (RabbitMQ), JWT.

API Layer (Carter)
- Project path: Services/Alias/Alias.API
- Folders:
  - Aliases/Features/: vertical slices per capability
    - IssueAlias/: create/issue an alias for a user
    - RenameAlias/: rename an existing alias (with validations/conflicts)
    - UpdateAliasVisibility/: set alias visibility (public/private)
    - FollowAlias/: follow an alias
    - UnfollowAlias/: unfollow an alias
    - GetFollowers/: list followers of an alias
    - GetFollowing/: list aliases the viewer follows
    - GetPublicProfile/: fetch public profile snapshot for an alias
  - Aliases/Models/: alias model types and value objects used in API
  - Aliases/Dtos/: request/response DTOs
  - Aliases/Exceptions/: domain/API exceptions
  - Aliases/Utils/: helpers
  - Common/: cross-cutting helpers (e.g., auth/claims)
  - Extensions/: service registrations (identity, swagger, cors)
  - Data/: persistence or external client adapters used by features
- Program.cs: config bootstrap, MapCarter(), UseSwagger(), UseAuthentication(), UseAuthorization().

Authentication & Authorization
- JWT-based auth via MinimalJwtValidation (from BuildingBlocks Extensions).
- ClaimsPrincipalExtensions (in Common/Authentication) provide helpers to extract alias/user identifiers.
- All write endpoints require RequireAuthorization().

Messaging (Shared BuildingBlocks)
- MassTransit configured via BuildingBlocks.Messaging.MassTransit.Extensions.AddMessageBroker when used.
- Shared integration events in BuildingBlocks.Messaging/Events/IntegrationEvents/Alias/:
  - AliasIssuedIntegrationEvent
  - AliasUpdatedIntegrationEvent
  - AliasVisibilityChangedIntegrationEvent
- Alias.API publishes these on relevant state changes so other services (Feed, Post, Profile) can react.

Follow Graph Interactions
- FollowAlias/UnfollowAlias endpoints update follow relationships; other services (e.g., Feed) consume a separate follow event if available to maintain their own projections.
- GetFollowers/GetFollowing are read endpoints optimized for paging; ensure AsNoTracking style reads when using EF or repo equivalents.

Data & Persistence
- Data folder encapsulates persistence access. Exact store type depends on implementation (not fully explored here). Use repositories or DbContexts consistent with the rest of the platform.

Quick Usage
- Issue Alias: POST /v1/aliases (requires auth) → returns alias details.
- Rename Alias: PUT /v1/aliases/{aliasId}/name
- Update Visibility: PUT /v1/aliases/{aliasId}/visibility
- Follow: POST /v1/aliases/{aliasId}/follow
- Unfollow: DELETE /v1/aliases/{aliasId}/follow
- Get Followers: GET /v1/aliases/{aliasId}/followers
- Get Following: GET /v1/aliases/{aliasId}/following
- Get Public Profile: GET /v1/aliases/{aliasId}/profile
(Note: Endpoint paths reflect common conventions; check specific Feature modules for exact routes.)

Key File Map
- Alias.API/Program.cs, Dependency and identity wiring in Extensions/.
- Alias.API/Aliases/Features/* (IssueAlias, RenameAlias, UpdateAliasVisibility, Follow/Unfollow, GetFollowers/Following, GetPublicProfile).
- Alias.API/Aliases/Dtos/* and Models/* for transport/domain types.
- Alias.API/Common/* for shared helpers (e.g., Auth claims accessors).

Definition of Done (DoD) for new Alias features
- Carter endpoint with Request/Response DTOs + RequireAuthorization for protected ops.
- CQRS request (Command/Query) + Validator where appropriate.
- Business logic encapsulated in dedicated services/domain helpers, not endpoints.
- Publish integration events (AliasIssued/Updated/VisibilityChanged) as needed.
- Swagger/OpenAPI docs for routes; unit tests for handlers; integration tests for endpoints.

Notes & Gaps
- Keep event contract versions aligned across services.
- Ensure username/alias uniqueness and proper conflict handling on rename.
- Rate-limit follow/unfollow and validate self-follow prevention rules.
- If follow events are introduced to BuildingBlocks, wire consumers in Feed to maintain projections.

