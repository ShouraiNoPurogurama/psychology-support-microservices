using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Test.Application.Dtos;
using Test.Application.Tests.Queries;

namespace Test.API.Endpoints
{
    public record GetAllTestsResponse(PaginatedResult<TestDto> Tests);

    public class GetAllTestsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/tests", async ([AsParameters] PaginationRequest request, ISender sender) =>
            {
                var query = new GetAllTestsQuery(request);
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllTestsResponse>();

                return Results.Ok(response);
            })
            .WithName("GetAllTests")
            .Produces<GetAllTestsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get all tests with pagination")
            .WithSummary("Get Paginated Tests");
        }
    }
}

