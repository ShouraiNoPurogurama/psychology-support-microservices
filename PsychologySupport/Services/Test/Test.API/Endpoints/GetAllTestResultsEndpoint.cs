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

namespace Test.API.Endpoints;

public record GetAllTestResultsRequest(
    int PageIndex = 1,
    int PageSize = 10,
    string? Search = "", // TestId
    string? SortBy = "TakenAt", // Sort TakenAt
    string? SortOrder = "asc", // Asc or Desc
    SeverityLevel? SeverityLevel = null);

public record GetAllTestResultsResponse(PaginatedResult<GetAllTestResultDto> TestResults);

public class GetAllTestResultsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/test-results/{patientId:guid}", async (
                [AsParameters] GetAllTestResultsRequest request, [FromRoute] Guid patientId,
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanViewPatientProfile(patientId, httpContext.User))
                    throw new ForbiddenException();

                var query = request.Adapt<GetAllTestResultsQuery>() with { PatientId = patientId };

                var result = await sender.Send(query);

                var response = new GetAllTestResultsResponse(result.TestResults);

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllTestResults")
            .WithTags("Test Results")
            .Produces<GetAllTestResultsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get All TestResults")
            .WithSummary("Get All TestResults");
    }
}