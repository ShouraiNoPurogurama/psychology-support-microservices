namespace Test.Application.Dtos;

public record TestResultDto(Guid Id, Guid TestId, int PatientId, DateTime TakenAt, string SeverityLevel);