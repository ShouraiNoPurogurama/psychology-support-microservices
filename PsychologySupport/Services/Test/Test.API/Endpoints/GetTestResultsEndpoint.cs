using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.TestOutput.Queries;

namespace Test.API.Endpoints
{
    public record GetTestResultRequest(Guid TestResultId);

    public record GetTestResultResponse(TestResultDto TestResult);

    public class GetTestResultsEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/test-result/{testResultId:guid}", async (Guid testResultId, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var query = new GetTestResultQuery(testResultId);
                var result = await sender.Send(query);
                var response = result.Adapt<GetTestResultResponse>();

                return Results.Ok(response);
            })
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Manager"))
                .WithName("GetTestResult")
                .WithTags("Test Results")
                .Produces<GetTestResultResponse>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Get Test Result")
                .WithSummary("Get Test Result");
        }
    }
}
