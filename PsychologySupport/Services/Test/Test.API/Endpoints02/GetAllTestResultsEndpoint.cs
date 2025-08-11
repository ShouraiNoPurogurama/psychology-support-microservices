using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.TestOutput.Queries;
using Test.Domain.Enums;

namespace Test.API.Endpoints02;

public record GetAllTestResultsV2Request(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = "", // TestId
    string? SortBy = "TakenAt", // Sort TakenAt
    string? SortOrder = "asc", // Asc or Desc
    SeverityLevel? SeverityLevel = null);

public record GetAllTestResultsV2Response(PaginatedResult<GetAllTestResultDto> TestResults);

public class GetAllTestResultsV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/me/testResults/{patientId:guid}", async (
                [AsParameters] GetAllTestResultsV2Request request, [FromRoute] Guid patientId,
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanViewPatientProfile(patientId, httpContext.User))
                    throw new ForbiddenException();

                var query = request.Adapt<GetAllTestResultsQuery>() with { PatientId = patientId };

                var result = await sender.Send(query);

                var response = new GetAllTestResultsV2Response(result.TestResults);

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllTestResults v2")
            .WithTags("Test Results Version 2")
            .Produces<GetAllTestResultsV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All TestResults")
            .WithSummary("Get All TestResults");
    }
}