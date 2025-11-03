using BuildingBlocks.Exceptions;
using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.Tests.Queries;

namespace Test.API.Endpoints.v1;

public record GetAllTestsResponse(PaginatedResult<TestDto> Tests);

public class GetAllTestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tests", async ([AsParameters] PaginationRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    throw new ForbiddenException();

                var query = new GetAllTestsQuery(request);
                var result = await sender.Send(query);
                var response = result.Adapt<GetAllTestsResponse>();

                return Results.Ok(response);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("GetAllTests")
            .WithTags("Tests")
            .Produces<GetAllTestsResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Get all tests with pagination")
            .WithSummary("Get Paginated Tests");
    }
}