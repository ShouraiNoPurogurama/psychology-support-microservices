using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Test.API.Common;
using Test.Application.TestOutput.Commands;

namespace Test.API.Endpoints;

public record CreateTestResultPdfRequest(
    Guid PatientId,
    Guid TestId,
    List<Guid> SelectedOptionIds,
    DateTime TakenAt,
    DateTime CompletedAt
);

public class CreateTestResultPdfEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/test-results/pdf", async (
                [FromBody] CreateTestResultPdfRequest request,
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientId, httpContext.User))
                    return Results.Forbid();

                var command = request.Adapt<CreateTestResultPdfCommand>();
                
                var result = await sender.Send(command);
                
                return Results.File(result.PdfBytes, "application/pdf", result.FileName);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreateTestResultAndReturnPdf")
            .WithTags("Test Results")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create a test result and return a PDF file")
            .WithSummary("Create Test Result and Export PDF");
    }
}