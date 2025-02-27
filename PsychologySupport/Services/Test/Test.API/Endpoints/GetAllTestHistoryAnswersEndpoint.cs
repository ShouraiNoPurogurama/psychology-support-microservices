using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Test.Application.TestOutput.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints
{
    public record GetAllTestHistoryAnswersResponse(PaginatedResult<TestHistoryAnswer> Answers);

    public class GetAllTestHistoryAnswersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/test-history-answers/{testResultId:guid}", async (
                Guid testResultId,
                [AsParameters] PaginationRequest request,
                ISender sender) =>
            {
                var query = new GetAllTestHistoryAnswersQuery(testResultId, request);
                var result = await sender.Send(query);
                var response = new GetAllTestHistoryAnswersResponse(result.Answers);

                return Results.Ok(response);
            })
            .WithName("GetAllTestHistoryAnswers")
            .Produces<GetAllTestHistoryAnswersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get paginated Test History Answers")
            .WithSummary("Get paginated Test History Answers");
        }
    }
}
