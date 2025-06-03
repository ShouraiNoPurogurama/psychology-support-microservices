using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Test.Application.TestOutput.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints;

public record GetAllTestResultsResponse(PaginatedResult<TestResult> TestResults);

public class GetAllTestResultsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/test-results/{patientId:guid}", async (
                [AsParameters] GetAllTestResultsQuery request,
                ISender sender) =>
            {
                var result = await sender.Send(request);
                var response = new GetAllTestResultsResponse(result.TestResults);

                return Results.Ok(response);
            })
            .WithName("GetAllTestResults")
            .WithTags("Test Results")
            .Produces<GetAllTestResultsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All TestResults")
            .WithSummary("Get All TestResults");
    }
}