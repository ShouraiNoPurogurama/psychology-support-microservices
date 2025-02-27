using Carter;
using MediatR;
using Test.Application.TestResults.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints
{
    public record GetAllTestHistoryAnswersResponse(IEnumerable<TestHistoryAnswer> Answers);

    public class GetAllTestHistoryAnswersEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/test-history-answers/{testResultId:guid}", async (Guid testResultId, ISender sender) =>
            {
                var query = new GetAllTestHistoryAnswersQuery(testResultId);
                var result = await sender.Send(query);
                var response = new GetAllTestHistoryAnswersResponse(result);

                return Results.Ok(response);
            })
            .WithName("GetAllTestHistoryAnswers")
            .Produces<GetAllTestHistoryAnswersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Test History Answers")
            .WithSummary("Get Test History Answers");
        }
    }
}
