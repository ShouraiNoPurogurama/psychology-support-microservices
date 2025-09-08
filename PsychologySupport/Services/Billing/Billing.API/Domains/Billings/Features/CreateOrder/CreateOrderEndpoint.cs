using Billing.API.Domains.Billings.Dtos;
using Billing.API.Domains.Billings.Features.CreateOrder;
using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Billing.API.Domains.Billings.Features.CreateOrder;


public record CreateOrderRequest(CreateOrderDto Order);

public record CreateOrderResponse(Guid OrderId, string InvoiceCode, string PaymentUrl, long? PaymentCode);

public class CreateOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (
            [FromBody] CreateOrderRequest request,
            [FromHeader(Name = "X-Idempotency-Key")] string idempotencyKeyHeader,
            ISender sender) =>
        {
            if (request?.Order == null)
                throw new BadRequestException("Request body is required.");

            if (!Guid.TryParse(idempotencyKeyHeader, out var idempotencyKey))
                throw new BadRequestException("Invalid X-Idempotency-Key header.");

            // Build command
            var command = new CreateOrderCommand(idempotencyKey,request.Order);

            // Send to handler
            var result = await sender.Send(command);

            // Adapt to response
            var response = result.Adapt<CreateOrderResponse>();

            return Results.Created($"/orders/{response.OrderId}", response);

        })
        .WithName("CreateOrder")
        .WithTags("Billing Orders")
        .Produces<CreateOrderResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status403Forbidden)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithDescription("Create a new order for point package purchase or subscription.")
        .WithSummary("Create Order");
    }
}
