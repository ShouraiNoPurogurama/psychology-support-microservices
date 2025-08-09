﻿using BuildingBlocks.CQRS;
using MediatR;
using Test.Application.Extensions.Utils;
using Test.Application.ServiceContracts;

namespace Test.Application.TestOutput.Commands;

public record CreateTestResultPdfCommand : ICommand<CreateTestResultPdfResult>
{
    public Guid PatientId { get; set; }
    public Guid TestId { get; set; }
    public List<Guid> SelectedOptionIds { get; set; } = [];
    public DateTime TakenAt { get; set; }
    public DateTime CompletedAt { get; set; }
}

public record CreateTestResultPdfResult(byte[] PdfBytes, string FileName);

public class CreateTestResultPdfHandler(ISender sender, ITestResultPdfService pdfService) : ICommandHandler<CreateTestResultPdfCommand, CreateTestResultPdfResult>
{
    public async Task<CreateTestResultPdfResult> Handle(CreateTestResultPdfCommand request, CancellationToken cancellationToken)
    {
        var createResultCommand = new CreateTestResultCommand
        {
            PatientId = request.PatientId,
            TestId = request.TestId,
            SelectedOptionIds = request.SelectedOptionIds
        };

        var testResult = (await sender.Send(createResultCommand, cancellationToken)).TestResult;

        var completeTime = request.CompletedAt - request.TakenAt;
        
        string formattedCompleteTime = DateTimeUtils.FormatCompletionTime(completeTime);

        var pdfBytes = pdfService.GeneratePdf(
            testResult.PatientName,
            request.TakenAt,
            testResult.PatientAge,
            testResult.ProfileNickname,
            testResult.ProfileDescription,
            testResult.ProfileHighlights,
            testResult.DepressionScore,
            testResult.AnxietyScore,
            testResult.StressScore,
            testResult.SeverityLevel,
            formattedCompleteTime,
            testResult.Recommendation
        );

        var fileName = $"DASS21_{testResult.PatientName}_{request.TakenAt:yyyyMMdd_HHmmss}.pdf";
        
        return new CreateTestResultPdfResult(pdfBytes, fileName);
    }
}