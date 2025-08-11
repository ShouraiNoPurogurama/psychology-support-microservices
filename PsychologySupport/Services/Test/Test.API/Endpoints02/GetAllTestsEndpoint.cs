using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.Tests.Queries;

namespace Test.API.Endpoints02;

public record GetAllTestsV2Response(PaginatedResult<TestDto> Tests);

public class GetAllTestsV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("v2/tests", async ([AsParameters] PaginationRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var query = new GetAllTestsQuery(request);
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllTestsV2Response>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllTests v2")
            .WithTags("Tests Version 2")
            .Produces<GetAllTestsV2Response>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get all tests with pagination")
            .WithSummary("Get Paginated Tests");
    }
}