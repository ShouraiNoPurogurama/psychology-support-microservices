using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.TestOutput.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints;

public record GetAllTestHistoryAnswersResponse(TestResultOptionsDto Answer);

public class GetAllTestHistoryAnswersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/test-history-answers/{testResultId:guid}", async (
                Guid testResultId,
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var query = new GetAllTestHistoryAnswersQuery(testResultId);
                var result = await sender.Send(query);
                var response = new GetAllTestHistoryAnswersResponse(result.Answer);

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllTestHistoryAnswers")
            .WithTags("Tests")
            .Produces<GetAllTestHistoryAnswersResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get paginated Test History Answers")
            .WithSummary("Get paginated Test History Answers");
    }
}