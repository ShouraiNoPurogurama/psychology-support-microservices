using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common;
using Wellness.Application.Features.ProcessHistories.Commands;
using Wellness.Domain.Enums;

namespace Wellness.API.Endpoints.ProcessHistories
{
    public record UpdateProcessHistoryRequest(
        Guid SubjectRef,
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
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check (nếu cần)
                // if (!AuthorizationHelpers.CanModify(request.SubjectRef, httpContext.User))
                //     throw new ForbiddenException();

                var command = request.Adapt<UpdateProcessHistoryCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateProcessHistoryResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateProcessHistory")
            .WithTags("ProcessHistories")
            .Produces<UpdateProcessHistoryResponse>(200)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update Process History")
            .WithDescription("Update status of a process history and set end time/post mood if completed.");
        }
    }
}
