using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common.Authentication;
using Wellness.Domain.Enums;

namespace Wellness.API.Endpoints.ModuleSections;

public record CreateModuleProgressRequest(
    Guid ModuleSectionId,
    Guid SectionArticleId // bài viết đầu tiên được đọc
);

public record CreateModuleProgressResponse(
    Guid ModuleProgressId,
    ProcessStatus ProcessStatus,
    int MinutesRead
);

public class CreateModuleProgressEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/me/module-progress", async (
            [FromBody] CreateModuleProgressRequest request,
            [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
            ICurrentActorAccessor currentActor,
            ISender sender
        ) =>
        {
            if (requestKey is null || requestKey == Guid.Empty)
                throw new BadRequestException(
                    "Missing or invalid Idempotency-Key header.",
                    "MISSING_IDEMPOTENCY_KEY"
                );

            var subjectRef = currentActor.GetRequiredSubjectRef();

            var command = new CreateModuleProgressCommand(
                IdempotencyKey: requestKey.Value,
                SubjectRef: subjectRef,
                ModuleSectionId: request.ModuleSectionId,
                SectionArticleId: request.SectionArticleId
            );

            var result = await sender.Send(command);

            var response = new CreateModuleProgressResponse(
                result.ModuleProgressId,
                result.ProcessStatus,
                result.MinutesRead
            );

            return Results.Created(
                $"/v1/me/module-progress/{response.ModuleProgressId}",
                response
            );
        })
        .RequireAuthorization()
        .WithName("CreateModuleProgress")
        .WithTags("ModuleProgress")
        .Produces<CreateModuleProgressResponse>(201)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Create a Module Progress entry for the current user")
        .WithDescription("Creates a new module progress record for the authenticated user. The 'Idempotency-Key' header ensures idempotent creation. The SectionArticleId must belong to the specified ModuleSection.");
    }
}
