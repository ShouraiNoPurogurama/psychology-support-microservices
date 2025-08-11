using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Test.API.Common;
using Test.Application.Tests.Queries;
using Test.Domain.Models;

namespace Test.API.Endpoints02;

public record GetAllQuestionOptionsV2Response(PaginatedResult<QuestionOption> QuestionOptions);

public class GetAllQuestionOptionsV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/questionOptions/{questionId:guid}", async (
                Guid questionId,
                [AsParameters] PaginationRequest request,
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var query = new GetAllQuestionOptionsQuery(questionId, request);
                var result = await sender.Send(query);
                var response = new GetAllQuestionOptionsV2Response(result.QuestionOptions);

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllQuestionOptions v2")
            .WithTags("Tests Version 2")
            .Produces<GetAllQuestionOptionsV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get paginated Question Options")
            .WithSummary("Get paginated Question Options");
    }
}