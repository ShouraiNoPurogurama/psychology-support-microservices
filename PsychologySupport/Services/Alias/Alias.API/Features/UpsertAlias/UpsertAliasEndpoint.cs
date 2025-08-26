using Carter;
using MediatR;

namespace Alias.API.Features.UpsertAlias;

public record UpsertAliasRequest();

public class UpsertAliasResponse();

public class UpsertAliasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("me/alias", async (UpsertAliasRequest request, ISender sender) =>
        {

        });
    }
}