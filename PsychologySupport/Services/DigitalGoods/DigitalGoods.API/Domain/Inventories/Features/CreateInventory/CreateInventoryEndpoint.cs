using BuildingBlocks.Exceptions;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using DigitalGoods.API.Domain.Inventories.Features.CreateInventory;

namespace DigitalGoods.API.Endpoints.Inventories;

public record CreateInventoryRequest(
    Guid SubjectRef,
    DateTimeOffset GrantedAt,
    DateTimeOffset ExpiredAt
);

public record CreateInventoryResponse(
    string Status,
    DateTimeOffset GrantedAt
);

public class CreateInventoryEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/inventories", async (
            [FromBody] CreateInventoryRequest request,
            ISender sender) =>
        {
            if (request.GrantedAt >= request.ExpiredAt)
                throw new BadRequestException("GrantedAt must be earlier than ExpiredAt.");

            var command = new CreateInventoryCommand(
                SubjectRef: request.SubjectRef,
                GrantedAt: request.GrantedAt,
                ExpiredAt: request.ExpiredAt
            );

            var result = await sender.Send(command);

            var response = new CreateInventoryResponse(
                result.Status,
                result.GrantedAt
            );

            return Results.Created(
                "/v1/inventories",
                response
            );
        })
        .RequireAuthorization()
        .WithName("CreateInventories")
        .WithTags("Inventories")
        .Produces<CreateInventoryResponse>(201)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Create Inventories")
        .WithDescription("Creates inventory records for all active digital goods for the given subject, with specified granted and expiry times.");
    }
}
