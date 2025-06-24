﻿using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Test.Application.TestOutput.Commands;

namespace Test.API.Endpoints;

public record CreateTestResultRequest(
    Guid PatientId,
    Guid TestId,
    List<Guid> SelectedOptionIds);

public record CreateTestResultResponse(Guid TestResultId, string AIRecommendations);

public class CreateTestResultEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/test-results", async ([FromBody] CreateTestResultRequest request, ISender sender) =>
            {
                var command = request.Adapt<CreateTestResultCommand>();

                var result = await sender.Send(command);

                var response = new CreateTestResultResponse(result.TestResultId, result.AIRecommendations);

                return Results.Ok(response);
            })
            .WithName("CreateTestResult")
            .WithTags("Test Results")
            .Produces<CreateTestResultResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create a new test result")
            .WithSummary("Create Test Result");
    }
}