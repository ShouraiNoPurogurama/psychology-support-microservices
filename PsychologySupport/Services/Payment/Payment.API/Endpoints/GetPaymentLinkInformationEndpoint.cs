using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.Payments.Queries;

namespace Payment.API.Endpoints
{

    public class GetPaymentLinkInformationEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/payments/payos/link-information/{paymentCode}", async (
                long paymentCode,
                [FromServices] ISender sender) =>
            {
                var query = new GetPaymentLinkInformationQuery(paymentCode);

                var result = await sender.Send(query);
                    return Results.Ok(result);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetPaymentLinkInformation")
            .WithTags("PayOS Payments")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Retrieves information about a specific payment link")
            .WithSummary("Get Payment Link Information");
        }
    }
}
