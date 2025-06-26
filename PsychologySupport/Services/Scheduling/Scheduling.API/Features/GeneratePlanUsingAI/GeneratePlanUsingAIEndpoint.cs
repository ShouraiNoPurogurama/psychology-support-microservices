using Carter;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Scheduling.API.Common;
using Scheduling.API.Dtos;
using Scheduling.API.Features.Schedule.ImportSchedule;
using System.Text.Json;

namespace Scheduling.API.Features.GeneratePlanUsingAI;

public class GeneratePlanUsingAiEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/openai");

        group.MapPost("/generate-plan", async (
            [FromBody] ScheduleRequest request,
            ISender sender, HttpContext httpContext) =>
        {
            // Authorization check
            if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientId, httpContext.User))
                return Results.Forbid();

            var scores = JsonSerializer.Serialize(request);

            GeneratePlanResult result = await sender.Send(new GeneratePlanCommand(scores));

            await sender.Send(new ImportScheduleCommand(
                request.PatientId,
                result.Plan
            ));

            return Results.Ok(new { plan = result });
        })
        .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
        .WithName("GeneratePlan")
        .WithTags("OpenAI")
        .Produces<GeneratePlanResult>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Generate Plan Using AI")
        .WithDescription("Generates a treatment plan using AI based on the provided schedule request.");
    }
}