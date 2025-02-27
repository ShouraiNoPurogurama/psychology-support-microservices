using Carter;
using MediatR;
using Test.Application.TestResults.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints
{
    public record GetAllTestResultsResponse(IEnumerable<TestResult> TestResults);

    public class GetAllTestResultsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/test-results/{patientId:guid}", async (Guid patientId, ISender sender) =>
            {
                var query = new GetAllTestResultsQuery(patientId);
                var result = await sender.Send(query);
                var response = new GetAllTestResultsResponse(result);

                return Results.Ok(response);
            })
            .WithName("GetAllTestResults")
            .Produces<GetAllTestResultsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get all test results for a specific patient")
            .WithSummary("Get Test Results");
        }
    }
}
