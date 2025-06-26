using Test.Application.ServiceContracts;
using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Infrastructure.Services.Pdf;

using QuestPDF.Fluent;

public class TestResultPdfService : ITestResultPdfService
{
    public byte[] GeneratePdf(string clientName, DateTime testDate, int age,
        Score depression, Score anxiety, Score stress, SeverityLevel severityLevel, string completeTime, Recommendation recommendation)
    {
        var document =
            new Dass21PdfDocument(clientName, testDate, age, depression, anxiety, stress, severityLevel, completeTime, recommendation);

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }
}