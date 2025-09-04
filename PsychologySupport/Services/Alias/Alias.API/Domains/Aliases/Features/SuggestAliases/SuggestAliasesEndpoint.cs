using Alias.API.Domains.Aliases.Dtos;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Domains.Aliases.Features.SuggestAliases;

public record SuggestAliasesRequest(TimeSpan Ttl, int PageIndex = 1, int PageSize = 5);

public record SuggestAliasesResponse(IReadOnlyList<SuggestAliasesItemDto> Aliases, DateTimeOffset GeneratedAt);

public class SuggestAliasesEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/aliases/suggest", async ([AsParameters] SuggestAliasesRequest request, ISender sender) =>
        {
            var query = new SuggestAliasesQuery(new PaginationRequest(request.PageIndex, request.PageSize), request.Ttl);

            var result = await sender.Send(query);

            var response = result.Adapt<SuggestAliasesResponse>();

            return Results.Ok(response);
        });
    }
}