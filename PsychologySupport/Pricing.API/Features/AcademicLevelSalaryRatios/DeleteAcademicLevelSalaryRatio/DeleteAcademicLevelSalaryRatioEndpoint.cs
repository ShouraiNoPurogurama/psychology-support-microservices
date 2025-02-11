using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Pricing.API.Features.AcademicLevelSalaryRatios.DeleteAcademicLevelSalaryRatio;

public record DeleteAcademicLevelSalaryRatioRequest(Guid Id);

public record DeleteAcademicLevelSalaryRatioResponse(bool IsSuccess);

public class DeleteAcademicLevelSalaryRatioEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/academic-level-salary-ratios", async ([FromBody] DeleteAcademicLevelSalaryRatioRequest request, ISender sender) =>
        {
            var result = await sender.Send(new DeleteAcademicLevelSalaryRatioCommand(request.Id));

            var response = result.Adapt<DeleteAcademicLevelSalaryRatioResponse>();

            return Results.Ok(response);
        })
        .WithName("DeleteAcademicLevelSalaryRatio")
        .Produces<DeleteAcademicLevelSalaryRatioResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithDescription("Delete Academic Level Salary Ratio")
        .WithSummary("Delete Academic Level Salary Ratio");
    }
}
