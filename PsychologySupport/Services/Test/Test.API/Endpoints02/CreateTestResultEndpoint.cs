using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.TestOutput.Commands;

namespace Test.API.Endpoints02;

public record CreateTestResultV2Request(
    Guid PatientId,
    Guid TestId,
    List<Guid> SelectedOptionIds
    );

public record CreateTestResultV2Response(TestResultDto TestResult);

public class CreateTestResultV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v2/me/testResult", async ([FromBody] CreateTestResultV2Request request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientId, httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<CreateTestResultCommand>();

                var result = await sender.Send(command);

                var response = new CreateTestResultV2Response(result.TestResult);

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreateTestResult v2")
            .WithTags("Test Results Version 2")
            .Produces<CreateTestResultV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create a new test result")
            .WithSummary("Create Test Result");
    }
}