using BuildingBlocks.Exceptions;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Wellness.API.Common;
using Wellness.Application.Features.JournalMoods.Dtos;
using Wellness.Application.Features.JournalMoods.Queries;

namespace Wellness.API.Endpoints.JournalMoods;

public record GetMoodsResponse(IEnumerable<MoodDto> Moods);

public class GetMoodEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/moods", async (ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            //if (!AuthorizationHelpers.HasViewAccess(httpContext.User))
            //    throw new ForbiddenException();

            var result = await sender.Send(new GetMoodsQuery());
            return Results.Ok(new GetMoodsResponse(result.Moods));
        })
        .WithName("GetMoods")
        .WithTags("Moods")
        .Produces<GetMoodsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Get all moods")
        .WithDescription("Returns the full list of available moods.");
    }
}
