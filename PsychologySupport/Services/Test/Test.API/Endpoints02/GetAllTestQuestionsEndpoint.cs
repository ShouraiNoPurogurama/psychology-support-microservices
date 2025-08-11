using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.Tests.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints02;

public record GetAllTestQuestionsV2Response(PaginatedResult<TestQuestionDto> TestQuestions);

public class GetAllTestQuestionsV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/testQuestions/{testId:guid}",
                async (Guid testId, [AsParameters] PaginationRequest request, ISender sender, HttpContext httpContext) =>
                {
                    // Authorization check
                    if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                        throw new ForbiddenException();

                    var query = new GetAllTestQuestionsQuery(testId, request);
                    
                    var result = await sender.Send(query);

                    var response = new GetAllTestQuestionsV2Response(result.TestQuestions);

                    return Results.Ok(response);
                })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllTestQuestions v2")
            .WithTags("Tests Version 2")
            .Produces<GetAllTestQuestionsV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get Test Questions")
            .WithSummary("Get Test Questions");
    }
}