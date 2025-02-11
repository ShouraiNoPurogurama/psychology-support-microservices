using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pricing.API.Modules;

namespace Pricing.API.Features.AcademicLevelSalaryRatios.CreateAcademicLevelSalaryRatio;

public record CreateAcademicLevelSalaryRatioRequest(AcademicLevelSalaryRatio SalaryRatio);

public record CreateAcademicLevelSalaryRatioResponse(Guid Id);

public class CreateAcademicLevelSalaryRatioEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/academic-level-salary-ratios", async ([FromBody] CreateAcademicLevelSalaryRatioRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateAcademicLevelSalaryRatioCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<CreateAcademicLevelSalaryRatioResponse>();

            return Results.Created($"/academic-level-salary-ratios/{response.Id}", response);
        })
            .WithName("CreateAcademicLevelSalaryRatio")
            .Produces<CreateAcademicLevelSalaryRatioResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create Academic Level Salary Ratio")
            .WithSummary("Create Academic Level Salary Ratio");
    }
}
