using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Test.API.Common;
using Test.Application.Tests.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints;

public record GetAllQuestionOptionsResponse(PaginatedResult<QuestionOption> QuestionOptions);

public class GetAllQuestionOptionsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/question-options/{questionId:guid}", async (
                Guid questionId,
                [AsParameters] PaginationRequest request,
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

                var query = new GetAllQuestionOptionsQuery(questionId, request);
                var result = await sender.Send(query);
                var response = new GetAllQuestionOptionsResponse(result.QuestionOptions);

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllQuestionOptions")
            .WithTags("Test")
            .Produces<GetAllQuestionOptionsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get paginated Question Options")
            .WithSummary("Get paginated Question Options");
    }
}