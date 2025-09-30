using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common;
using Wellness.Application.Features.Challenges.Commands;
using Wellness.Application.Features.ProcessHistories.Commands;

namespace Wellness.API.Endpoints.ProcessHistories;

public record CreateProcessHistoryRequest(
    Guid SubjectRef,
    Guid ActivityId
);

public record CreateProcessHistoryResponse(
    Guid ProcessHistoryId,
    string Status,
    DateTimeOffset StartTime
);

public class CreateProcessHistoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/me/process-histories", async (
            [FromBody] CreateProcessHistoryRequest request,
            [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
            ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            //if (!AuthorizationHelpers.CanModify(request.SubjectRef, httpContext.User))
            //    throw new ForbiddenException();

            if (requestKey is null || requestKey == Guid.Empty)
                throw new BadRequestException(
                    "Missing or invalid Idempotency-Key header.",
                    "MISSING_IDEMPOTENCY_KEY"
                );

            var command = new CreateProcessHistoryCommand(
                 IdempotencyKey: requestKey.Value,
                 SubjectRef: request.SubjectRef,
                 ActivityId: request.ActivityId
              );

            var result = await sender.Send(command);

            var response = new CreateProcessHistoryResponse(
                result.ProcessHistoryId,
                result.Status.ToString(),
                result.StartTime
            );

            return Results.Created(
                $"/v1/me/process-histories/{response.ProcessHistoryId}",
                response
            );
        })
        //.RequireAuthorization()
        .WithName("CreateProcessHistory")
        .WithTags("ProcessHistories")
        .Produces<CreateProcessHistoryResponse>(201)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Create Process History")
        .WithDescription("Initialize a new Process History for a subject and optional activity.");
    }
}
