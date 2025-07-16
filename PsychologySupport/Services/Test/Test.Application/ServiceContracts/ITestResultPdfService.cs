using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Application.ServiceContracts;

public interface ITestResultPdfService
{
    byte[] GeneratePdf(string clientName, DateTime testDate, int age, string profileNickname, string profileDescription, List<string> profileHighlights, Score depression, Score anxiety, Score stress, SeverityLevel severityLevel, string completeTime, Recommendation recommendation);
}
