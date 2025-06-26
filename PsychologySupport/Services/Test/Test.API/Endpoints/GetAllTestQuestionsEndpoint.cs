using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Test.Application.Dtos;
using Test.Application.Tests.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints;

public record GetAllTestQuestionsResponse(PaginatedResult<TestQuestionDto> TestQuestions);

public class GetAllTestQuestionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/test-questions/{testId:guid}",
                async (Guid testId, [AsParameters] PaginationRequest request, ISender sender) =>
                {
                    var query = new GetAllTestQuestionsQuery(testId, request);
                    
                    var result = await sender.Send(query);

                    var response = new GetAllTestQuestionsResponse(result.TestQuestions);

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllTestQuestions")
            .WithTags("Tests")
            .Produces<GetAllTestQuestionsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Test Questions")
            .WithSummary("Get Test Questions");
    }
}