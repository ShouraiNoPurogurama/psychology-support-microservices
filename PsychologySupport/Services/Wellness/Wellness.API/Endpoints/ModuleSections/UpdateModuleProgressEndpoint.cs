using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.Domain.Enums;

namespace Wellness.API.Endpoints.ModuleSections
{
    public record UpdateModuleProgressRequest(
        Guid ModuleSectionId,
        Guid SubjectRef,
        Guid ArticleId
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
            app.MapPut("/v1/module-progress", async (
                [FromBody] UpdateModuleProgressRequest request,
                ISender sender) =>
            {
                var command = request.Adapt<UpdateModuleProgressCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<UpdateModuleProgressResponse>();

                return Results.Ok(response);
            })
            .WithName("UpdateModuleProgress")
            .WithTags("ModuleProgress")
            .Produces<UpdateModuleProgressResponse>(200)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("ModuleProgress")
            .WithDescription("ModuleProgress");
        }
    }
}
