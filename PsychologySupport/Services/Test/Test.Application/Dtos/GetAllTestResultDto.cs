using Test.Domain.Enums;
using Test.Domain.ValueObjects;

namespace Test.Application.Dtos;

public record GetAllTestResultDto(Guid Id,
    Guid TestId,
    Guid PatientId,
    DateTime TakenAt,
    SeverityLevel SeverityLevel,
    Score DepressionScore,
    Score AnxietyScore,
    Score StressScore,
    Recommendation Recommendation);