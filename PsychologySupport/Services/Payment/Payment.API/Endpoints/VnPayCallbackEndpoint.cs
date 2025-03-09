using Carter;
using MediatR;
using Payment.Application.Payments.Dtos;
using Payment.Application.Payments.Queries;

namespace Payment.API.Endpoints;

public record VnPayCallbackRequest(VnPayCallbackDto VnPayCallback);

public class VnPayCallbackEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("payments/callback", async (VnPayCallbackRequest request, ISender sender) =>
        {
            var query = new VnPayCallbackQuery(request.VnPayCallback);

            var result = await sender.Send(query);

            return Results.Ok(result);
        });
    }
}