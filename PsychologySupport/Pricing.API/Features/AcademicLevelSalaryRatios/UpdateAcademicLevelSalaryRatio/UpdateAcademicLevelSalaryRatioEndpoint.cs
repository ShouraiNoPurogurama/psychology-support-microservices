using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Pricing.API.Dtos;

namespace Pricing.API.Features.AcademicLevelSalaryRatios.UpdateAcademicLevelSalaryRatio;

public record UpdateAcademicLevelSalaryRatioRequest(AcademicLevelSalaryRatioDto AcademicLevelSalaryRatio);

public record UpdateAcademicLevelSalaryRatioResponse(bool IsSuccess);

public class UpdateAcademicLevelSalaryRatioEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("academic-level-salary-ratio", async ([FromBody] UpdateAcademicLevelSalaryRatioRequest request, ISender sender) =>
        {
            var command = request.Adapt<UpdateAcademicLevelSalaryRatioCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<UpdateAcademicLevelSalaryRatioResponse>();

            return Results.Ok(response);
        })
        .WithName("UpdateAcademicLevelSalaryRatio")
        .Produces<UpdateAcademicLevelSalaryRatioResponse>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Update Academic Level Salary Ratio")
        .WithSummary("Update Academic Level Salary Ratio");
    }
}
