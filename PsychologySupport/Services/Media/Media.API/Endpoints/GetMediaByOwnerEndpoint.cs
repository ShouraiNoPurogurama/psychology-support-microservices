using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using Media.Application.Features.Media.Dtos;
using Media.Application.Features.Media.Queries;
using Media.Domain.Enums;
using MediatR;


namespace Media.API.Endpoints
{
     public record GetMediaByOwnerRequest(
        MediaOwnerType OwnerType,
        Guid OwnerId,
        int PageIndex = 1,
        int PageSize = 10
     );

    public record GetMediaByOwnerResponse(PaginatedResult<MediaByOwnerDto> Media);

    public class GetMediaByOwnerEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/media/owners", async (
                    [AsParameters] GetMediaByOwnerRequest request,
                    ISender sender,
                    HttpContext httpContext) =>
            {
                //// Authorization check
                //if (!httpContext.User.IsInRole("User") && !httpContext.User.IsInRole("Admin"))
                //    throw new ForbiddenException();

                var query = request.Adapt<GetMediaByOwnerQuery>();

                var result = await sender.Send(query);

                var response = new GetMediaByOwnerResponse(result.Media);

                return Results.Ok(response);
            })
                //.RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
                .WithName("GetMediaByOwner")
                .WithTags("Media")
                .Produces<GetMediaByOwnerResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("GetMediaByOwner")
                .WithSummary("GetMediaByOwnerr");
        }
    }
}
