using System.Security.Claims;
using Alias.API.Enums;
using Alias.API.Extensions;
using Carter;
using Mapster;
using MediatR;

namespace Alias.API.Features.UpsertAlias;

public record UpsertAliasRequest(
    string AliasLabel,
    string Token,
    NicknameSource NicknameSource
    );

public record UpsertAliasResponse(Guid AliasId);

public class UpsertAliasEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("me/alias", async (UpsertAliasRequest request, ClaimsPrincipal user, ISender sender) =>
        {
            var aliasId = user.GetAliasId();
            
            var command = new UpsertAliasCommand(aliasId, request.AliasLabel, request.Token, request.NicknameSource);

            var result = await sender.Send(command);

            var response = result.Adapt<UpsertAliasResponse>();

            return Results.Ok(response);
        });
    }
}