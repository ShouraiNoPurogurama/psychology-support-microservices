using QuestPDF.Companion;
using Test.Application.ServiceContracts;
using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Infrastructure.Services.Pdf;

using QuestPDF.Fluent;

public class TestResultPdfService(Dass21PercentileLookup lookup) : ITestResultPdfService
{

    public byte[] GeneratePdf(string clientName, DateTime testDate, int age, string profileNickname, string profileDescription, List<string> profileHighlights,
        Score depression, Score anxiety, Score stress, SeverityLevel severityLevel, string completeTime, Recommendation recommendation)
    {
        var document =
            new Dass21PdfDocument(clientName, testDate, age, profileNickname, profileDescription, profileHighlights, depression, anxiety, stress, severityLevel, completeTime, recommendation, lookup);

        using var stream = new MemoryStream();
        // document.ShowInCompanion();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }
}