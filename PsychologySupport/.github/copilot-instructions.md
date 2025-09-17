# Coding Agent Prompt (Aligned with Project Conventions)

This prompt enforces **.NET DDD/CQRS conventions** as defined in the Project Manifesto.  
All endpoint code must comply with **domain-driven rules, CQRS layering, DTO usage, validation, mapping, and event handling**.

---

##  Mission
Produce **correct, convention-driven code**.  
Each endpoint must include:
- `record Request` and `record Response` types (no anonymous or dynamic payloads).
- Use of existing DTOs and Enums if available; avoid creating unnecessary DTOs or magic strings.
- Mapping, Validation, Events, Migrations as required.

---

##  Endpoint Rules

1. **Structure**
    - Endpoint class per feature (e.g., `CreatePostEndpoint.cs`).
    - Two records:
      ```csharp
      public record CreatePostRequest(string Title, string Content);
      public record CreatePostResponse(Guid PostId, string Title);
      ```
    - No business logic in the endpoint → send Command/Query via `IMediator`.

2. **DTOs & Mapping**
    - Reuse existing DTOs if available.
    - Update `MapsterConfigurations.cs` for new mappings.
    - No EF entities returned directly → always map to DTOs/Responses.

3. **Validation**
    - Every Command/Query must have a `*Validator.cs` using FluentValidation.
    - Validate IDs, required fields, string lengths, limits, enums.

4. **Domain & Application Layers**
    - Commands → `IPostDbContext` for writes.
    - Queries → `IPostDbContext` for domain reads; `IQueryDbContext` only for replicas in projections.
    - Aggregates enforce invariants; handlers orchestrate.

5. **Events & Outbox**
    - Publish integration events via Outbox for consistency-critical flows.
    - Use typed event contracts under `BuildingBlocks.Messaging`.

6. **Queries**
    - Always `AsNoTracking()` for reads.
    - Use Pagination for all list queries (`PaginationRequest`, `PaginatedResult<T>`).
    - No cross-joins between contexts.

---

##  Workflow for Coding Agent

1. **Understand the Feature**
    - Read user request carefully.
    - Identify Command vs Query, Aggregates, DTOs, Enums involved.

2. **Plan the Files**
    - List all files to modify or create:
        - `*Endpoint.cs`
        - `*Command.cs` / `*Query.cs`
        - `*Handler.cs`
        - `*Validator.cs`
        - `*Dto.cs` (if new needed)
        - `MapsterConfigurations.cs`
        - `Migrations` (if DB changes)
        - `IntegrationEvents/*` (if events emitted)

3. **Generate Endpoint**
    - Carter or Minimal API endpoint → IMediator → Command/Query Handler → DTO Response.

4. **Implement Command/Query & Handler**
    - Command/Query record with properties.
    - Handler uses right DbContext, aggregates, mapping, events.

5. **Validation**
    - FluentValidation class per Command/Query.

6. **Mapping**
    - Mapster configuration for DTO <-> Entity.

7. **Events & Migrations**
    - Outbox event if needed.
    - Add migration with indexes & constraints.

8. **Testing**
    - Unit tests for handler & validator.
    - Integration tests for DB and event flows.

---

##  Definition of Done

- [ ] Endpoint has `Request` & `Response` records (no anonymous types).
- [ ] Correct DbContext usage (`IPostDbContext` vs `IQueryDbContext`).
- [ ] Validation via FluentValidation exists.
- [ ] DTOs reused or minimally created; no magic strings.
- [ ] Mapster mappings updated.
- [ ] Queries use `AsNoTracking()` + Pagination.
- [ ] Outbox events published if needed.
- [ ] Migrations added for schema changes.
- [ ] Unit & integration tests written.

---

##  Coding Conventions Recap

- **Endpoints**: Thin, only mediate → IMediator → Application Layer.
- **Commands/Queries**: Orchestrate via Handlers; Aggregates enforce rules.
- **DTOs**: Always map EF entities → DTOs → Responses.
- **Events**: Typed, versioned contracts under `BuildingBlocks.Messaging`.
- **Validation**: Mandatory for all inputs.
- **Pagination**: Required for collections.
- **No Cross-Joins**: Never join `IPostDbContext` and `IQueryDbContext`.

---
