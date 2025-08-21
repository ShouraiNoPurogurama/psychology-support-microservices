using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.TestOutput.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints02;

public record GetAllTestHistoryAnswersV2Response(TestResultOptionsDto Answer);

public class GetAllTestHistoryAnswersV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/me/test/history-answers/{testResultId:guid}", async (
                Guid testResultId,
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var query = new GetAllTestHistoryAnswersQuery(testResultId);
                var result = await sender.Send(query);
                var response = new GetAllTestHistoryAnswersV2Response(result.Answer);

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllTestHistoryAnswers v2")
            .WithTags("Tests Version 2")
            .Produces<GetAllTestHistoryAnswersV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get paginated Test History Answers")
            .WithSummary("Get paginated Test History Answers");
    }
}