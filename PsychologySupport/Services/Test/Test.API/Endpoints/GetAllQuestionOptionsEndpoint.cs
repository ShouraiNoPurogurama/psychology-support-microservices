using Carter;
using MediatR;
using Test.Application.Tests.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints
{
    public record GetAllQuestionOptionsResponse(IEnumerable<QuestionOption> QuestionOptions);

    public class GetAllQuestionOptionsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/question-options/{questionId:guid}", async (Guid questionId, ISender sender) =>
            {
                var query = new GetAllQuestionOptionsQuery(questionId);
                var result = await sender.Send(query);
                var response = new GetAllQuestionOptionsResponse(result);

                return Results.Ok(response);
            })
            .WithName("GetAllQuestionOptions")
            .Produces<GetAllQuestionOptionsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Question Options")
            .WithSummary("Get Question Options");
        }
    }
}
