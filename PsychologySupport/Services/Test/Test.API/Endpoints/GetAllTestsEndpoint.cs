using BuildingBlocks.Pagination;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Test.API.Common;
using Test.Application.Dtos;
using Test.Application.Tests.Queries;

namespace Test.API.Endpoints;

public record GetAllTestsResponse(PaginatedResult<TestDto> Tests);

public class GetAllTestsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/tests", async ([AsParameters] PaginationRequest request, ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.HasViewAccessToPatientProfile(httpContext.User))
                    return Results.Problem(
                               statusCode: StatusCodes.Status403Forbidden,
                               title: "Forbidden",
                               detail: "You do not have permission to access this resource."
                           );

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