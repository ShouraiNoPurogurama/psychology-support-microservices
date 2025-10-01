using Billing.Application.Dtos;
using MediatR;


namespace Billing.API.Endpoints;
public record GetOrderRequest(Guid OrderId);

public record GetOrderResponse(
    Guid OrderId,
    string OrderType,
    decimal Amount,
    string Currency,
    string Status,
    string? PromoCode,
    DateTimeOffset CreatedAt,
    InvoiceDto Invoice
);
public static class GetOrderEndpoint
{
    public static void MapGetOrderEndpoint(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/v1/me/orders/{orderId}", async (
            Guid orderId,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var request = new GetOrderRequest(orderId);

            var result = await sender.Send(new GetOrderQuery(request.OrderId), cancellationToken);

            return Results.Ok(result);
        })
        .WithName("GetOrder")
        .WithTags("Orders")
        .Produces<GetOrderResponse>()
        .Produces(StatusCodes.Status404NotFound)
        .WithDescription("Get a single order by Id")
        .WithSummary("GetOrder");
    }
}
