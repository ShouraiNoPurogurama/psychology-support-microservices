using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Application.Dtos;

public record TestResultDto(
    Guid Id,
    Guid TestId,
    Guid PatientId,
    DateTime TakenAt,
    SeverityLevel SeverityLevel,
    Score DepressionScore,
    Score AnxietyScore,
    Score StressScore,
    string Recommendation);