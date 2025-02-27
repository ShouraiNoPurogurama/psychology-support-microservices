using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Test.Application.TestOutput.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints
{
    public record GetAllTestResultsResponse(PaginatedResult<TestResult> TestResults);

    public class GetAllTestResultsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/test-results/{patientId:guid}", async (
                Guid patientId,
                [AsParameters] PaginationRequest request,
                ISender sender) =>
            {
                var query = new GetAllTestResultsQuery(patientId, request);
                var result = await sender.Send(query);
                var response = new GetAllTestResultsResponse(result.TestResults);

                return Results.Ok(response);
            })
            .WithName("GetAllTestResults")
            .Produces<GetAllTestResultsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get paginated test results for a specific patient")
            .WithSummary("Get Paginated Test Results");
        }
    }
}
