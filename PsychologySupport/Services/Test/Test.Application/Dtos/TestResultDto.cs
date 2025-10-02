using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Application.Dtos;

public record TestResultDto(
    Guid Id,
    Guid TestId,
    Guid PatientId,
    DateTimeOffset TakenAt,
    SeverityLevel SeverityLevel,
    Score DepressionScore,
    Score AnxietyScore,
    Score StressScore,
    Recommendation Recommendation,
    string PatientName,
    int PatientAge,
    string ProfileNickname,
    string ProfileDescription,
    List<string> ProfileHighlights
    );