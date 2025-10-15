using Carter;
using DigitalGoods.API.Dtos;
using DigitalGoods.API.Enums;
using MediatR;

namespace DigitalGoods.API.Features.DigitalGoods.GetDigitalGood;

public record GetDigitalGoodsRequest(Guid SubjectRef, DigitalGoodType? Type = null);

public record GetDigitalGoodsResponse(List<DigitalGoodDto> DigitalGoods);

public class GetDigitalGoodEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/v1/digital-goods", async (
            [AsParameters] GetDigitalGoodsRequest request,
            ISender sender,
            HttpContext httpContext) =>
        {
            var query = new GetDigitalGoodsQuery(request.SubjectRef, request.Type);
            var result = await sender.Send(query);
            return Results.Ok(new GetDigitalGoodsResponse(result.DigitalGoods));
        })
        .WithName("GetDigitalGoods")
        .WithTags("DigitalGoods")
        .Produces<GetDigitalGoodsResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get DigitalGoods with optional type filter")
        .WithDescription("Returns all active DigitalGoods, optionally filtered by Type, with ownership status for the subject.");
    }
}