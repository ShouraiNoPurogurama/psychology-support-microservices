using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common.Authentication;
using Wellness.Application.Features.ProcessHistories.Commands;
using Wellness.Domain.Enums;

namespace Wellness.API.Endpoints.ProcessHistories
{
    public record UpdateProcessHistoryRequest(
        Guid ProcessHistoryId,
        ProcessStatus ProcessStatus,
        Guid? PostMoodId
    );

    public record UpdateProcessHistoryResponse(
        Guid ProcessHistoryId,
        ProcessStatus ProcessStatus,
        DateTimeOffset? EndTime,
        Guid? PostMoodId
    );

    public class UpdateProcessHistoryEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/v1/me/process-history", async (
                [FromBody] UpdateProcessHistoryRequest request,
                ICurrentActorAccessor currentActor, 
                ISender sender
            ) =>
            {
                var subjectRef = currentActor.GetRequiredSubjectRef();

                var command = new UpdateProcessHistoryCommand(
                    SubjectRef: subjectRef,
                    ProcessHistoryId: request.ProcessHistoryId,
                    ProcessStatus: request.ProcessStatus,
                    PostMoodId: request.PostMoodId
                );

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateProcessHistoryResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("UpdateProcessHistory")
            .WithTags("ProcessHistories")
            .Produces<UpdateProcessHistoryResponse>(200)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update process history for the current user")
            .WithDescription("Updates the process history (status, end time, and mood) for the authenticated user.");
        }
    }
}
