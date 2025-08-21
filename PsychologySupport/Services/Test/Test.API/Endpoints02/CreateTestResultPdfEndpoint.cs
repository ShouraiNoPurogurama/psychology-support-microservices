﻿using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Test.API.Common;
using Test.Application.TestOutput.Commands;

namespace Test.API.Endpoints02;

public record CreateTestResultPdfV2Request(
    Guid PatientId,
    Guid TestId,
    List<Guid> SelectedOptionIds,
    DateTime TakenAt,
    DateTime CompletedAt
);

public class CreateTestResultPdfV2Endpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v2/me/test/result/pdf", async (
                [FromBody] CreateTestResultPdfV2Request request,
                ISender sender, HttpContext httpContext) =>
            {
                // Authorization check
                if (!AuthorizationHelpers.CanModifyPatientProfile(request.PatientId, httpContext.User))
                    throw new ForbiddenException();

                var command = request.Adapt<CreateTestResultPdfCommand>();
                
                var result = await sender.Send(command);
                
                return Results.File(result.PdfBytes, "application/pdf", result.FileName);
            })
            .RequireAuthorization(policy => policy.RequireRole("User", "Admin"))
            .WithName("CreateTestResultAndReturnPdf v2")
            .WithTags("Test Results Version 2")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("Create a test result and return a PDF file")
            .WithSummary("Create Test Result and Export PDF");
    }
}