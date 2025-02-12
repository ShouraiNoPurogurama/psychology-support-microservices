using Carter;
using Mapster;
using MediatR;
using Pricing.API.Models;

namespace Pricing.API.Features.AcademicLevelSalaryRatios.GetAcademicLevelSalaryRatio
{
    public record GetAcademicLevelSalaryRatioResponse(AcademicLevelSalaryRatio AcademicLevelSalaryRatio);

    public class GetAcademicLevelSalaryRatioEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/academic-level-salary-ratio/{id}", async (Guid id, ISender sender) =>
            {
                var query = new GetAcademicLevelSalaryRatioQuery(id);

                var result = await sender.Send(query);

                var response = result.Adapt<GetAcademicLevelSalaryRatioResponse>();

                return Results.Ok(response);
            })
                .WithName("GetAcademicLevelSalaryRatio")
                .Produces<GetAcademicLevelSalaryRatioResponse>()
                .ProducesProblem(StatusCodes.Status404NotFound)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithDescription("Get Academic Level Salary Ratio")
                .WithSummary("Get Academic Level Salary Ratio");
        }
    }
}
