using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Wellness.API.Common;
using Wellness.Application.Features.JournalMoods.Dtos;
using Wellness.Application.Features.JournalMoods.Queries;

namespace Wellness.API.Endpoints.JournalMoods;

public record GetJournalMoodsRequest(
    Guid SubjectRef,
    int PageIndex = 1,
    int PageSize = 10,
    string SortDirection = "desc" // sort theo CreatedAt ("asc" hoặc "desc")
);

public record GetJournalMoodsResponse(PaginatedResult<JournalMoodDto> Moods);

public class GetJournalMoodEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/me/journal-moods", async (
            [AsParameters] GetJournalMoodsRequest request,
            ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            if (!AuthorizationHelpers.CanView(request.SubjectRef, httpContext.User))
                throw new ForbiddenException();

            var query = new GetJournalMoodsQuery(
                request.SubjectRef,
                request.PageIndex,
                request.PageSize,
                request.SortDirection
            );

            var result = await sender.Send(query);

            return Results.Ok(new GetJournalMoodsResponse(result.Moods));
        })
        .WithName("GetJournalMoods")
        .WithTags("JournalMoods")
        .Produces<GetJournalMoodsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get paginated JournalMoods by SubjectRef")
        .WithDescription("Returns paginated list of JournalMoods filtered by SubjectRef, sorted by CreatedAt.");
    }
}
