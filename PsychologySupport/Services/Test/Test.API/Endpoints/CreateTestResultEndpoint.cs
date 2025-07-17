using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.TestOutput.Commands;

namespace Test.API.Endpoints;

public record CreateTestResultRequest(
    Guid PatientId,
    Guid TestId,
    List<Guid> SelectedOptionIds
    );

public record CreateTestResultResponse(TestResultDto TestResult);

public class CreateTestResultEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/test-results", async ([FromBody] CreateTestResultRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientId, httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<CreateTestResultCommand>();

                var result = await sender.Send(command);

                var response = new CreateTestResultResponse(result.TestResult);

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreateTestResult")
            .WithTags("Test Results")
            .Produces<CreateTestResultResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create a new test result")
            .WithSummary("Create Test Result");
    }
}