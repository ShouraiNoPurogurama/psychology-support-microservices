using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pricing.API.Modules;

namespace Pricing.API.Features.AcademicLevelSalaryRatios.GetAcademicLevelSalaryRatios;

public record GetAcademicLevelSalaryRatiosRequest(int PageNumber, int PageSize);

public record GetAcademicLevelSalaryRatiosResponse(IEnumerable<AcademicLevelSalaryRatio> SalaryRatios, int TotalCount);

public class GetAcademicLevelSalaryRatiosEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("academic-level-salary-ratios", async ([FromQuery] int pageNumber, [FromQuery] int pageSize, ISender sender) =>
        {
            var query = new GetAcademicLevelSalaryRatiosQuery(pageNumber, pageSize);

            var result = await sender.Send(query);

            var response = result.Adapt<GetAcademicLevelSalaryRatiosResponse>();
            return Results.Ok(response);
        })
        .WithName("GetAcademicLevelSalaryRatios")
        .Produces<GetAcademicLevelSalaryRatiosResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Get Academic Level Salary Ratios")
        .WithSummary("Get Academic Level Salary Ratios");
    }
}
