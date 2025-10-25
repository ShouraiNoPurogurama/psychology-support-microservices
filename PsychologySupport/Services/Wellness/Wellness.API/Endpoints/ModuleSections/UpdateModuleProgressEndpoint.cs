using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common.Authentication;
using Wellness.Domain.Enums;

namespace Wellness.API.Endpoints.ModuleSections
{
    public record UpdateModuleProgressRequest(
        Guid ModuleSectionId,
        Guid SectionArticleId
    );

    public record UpdateModuleProgressResponse(
        Guid ModuleProgressId,
        ProcessStatus ProcessStatus,
        int MinutesRead
    );

    public class UpdateModuleProgressEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPut("/v1/me/module-progress", async (
                [FromBody] UpdateModuleProgressRequest request,
                ICurrentActorAccessor currentActor,  
                ISender sender
            ) =>
            {
                var subjectRef = currentActor.GetRequiredSubjectRef();

                var command = new UpdateModuleProgressCommand(
                    SubjectRef: subjectRef,
                    ModuleSectionId: request.ModuleSectionId,
                    SectionArticleId: request.SectionArticleId
                );

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateModuleProgressResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization()
            .WithName("UpdateModuleProgress")
            .WithTags("ModuleProgress")
            .Produces<UpdateModuleProgressResponse>(200)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("Update module progress for the current user")
            .WithDescription("Updates module progress (minutes read, process status) for the authenticated user.");
        }
    }
}
