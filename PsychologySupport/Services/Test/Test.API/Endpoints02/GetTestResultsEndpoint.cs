using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.TestOutput.Queries;

namespace Test.API.Endpoints02
{
    public record GetTestResultRequest(Guid TestResultId);

    public record GetTestResultV2Response(TestResultDto TestResult);

    public class GetTestResultsV2Endpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/v2/me/test/result/{testResultId:guid}", async (Guid testResultId, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var query = new GetTestResultQuery(testResultId);
                var result = await sender.Send(query);
                var response = result.Adapt<GetTestResultV2Response>();

                return Results.Ok(response);
            })
                .RequireAuthorization(policy => policy.RequireRole("User", "Admin","Manager"))
                .WithName("GetTestResult v2")
                .WithTags("Test Results Version 2")
                .Produces<GetTestResultV2Response>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .WithDescription("Get Test Result")
                .WithSummary("Get Test Result");
        }
    }
}
