using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Payment.Application.Payments.Dtos;
using Payment.Application.Payments.Queries;

namespace Payment.API.Endpoints
{
    public record GetAllPaymentsResponse(PaginatedResult<PaymentDto> Payments);

    public class GetAllPaymentsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/payments", async ([AsParameters] GetAllPaymentsQuery request, ISender sender) =>
            {
                var result = await sender.Send(request);
                var response = result.Adapt<GetAllPaymentsResponse>();
                return Results.Ok(response);
            })
            .WithName("GetAllPayments")
            .WithTags("Payments")
            .Produces<GetAllPaymentsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All Payments")
            .WithSummary("Get All Payments");
        }
    }
}
