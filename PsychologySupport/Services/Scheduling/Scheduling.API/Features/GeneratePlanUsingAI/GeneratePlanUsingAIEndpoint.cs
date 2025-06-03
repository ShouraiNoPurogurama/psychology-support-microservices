using System.Text.Json;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Dtos;
using Scheduling.API.Features.Schedule.ImportSchedule;

namespace Scheduling.API.Features.GeneratePlanUsingAI;

public class GeneratePlanUsingAiEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/openai");

        group.MapPost("/generate-plan", async (
            [FromBody] ScheduleRequest request,
            ISender sender) =>
        {
            var scores = JsonSerializer.Serialize(request);

            GeneratePlanResult result = await sender.Send(new GeneratePlanCommand(scores));

            await sender.Send(new ImportScheduleCommand(
                request.PatientId,
                result.Plan
            ));

            return Results.Ok(new { plan = result });
        })
        .WithName("GeneratePlan")
        .WithTags("OpenAI")
        .Produces<GeneratePlanResult>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Generate Plan Using AI")
        .WithDescription("Generates a treatment plan using AI based on the provided schedule request.");
    }
}