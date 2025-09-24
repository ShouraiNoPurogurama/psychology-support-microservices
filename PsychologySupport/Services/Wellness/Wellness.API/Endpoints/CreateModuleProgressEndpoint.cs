using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.Application.Features.ModuleSections.Commands;
using Wellness.Domain.Enums;

namespace Wellness.API.Endpoints
{
    public record CreateModuleProgressRequest(
        Guid SubjectRef,
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
            app.MapPost("/v1/module-progress", async (
                [FromBody] CreateModuleProgressRequest request,
                ISender sender) =>
            {

                var command = request.Adapt<CreateModuleProgressCommand>();


                var result = await sender.Send(command);


                var response = result.Adapt<CreateModuleProgressResponse>();

                return Results.Created(
                    $"/v1/module-progress/{response.ModuleProgressId}",
                    response
                );
            })
            .WithName("CreateModuleProgress")
            .WithTags("ModuleProgress")
            .Produces<CreateModuleProgressResponse>(201)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithSummary("ModuleProgress")
            .WithDescription("ModuleProgress");
        }
    }
}
