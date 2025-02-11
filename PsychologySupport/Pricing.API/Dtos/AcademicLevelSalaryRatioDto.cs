namespace Pricing.API.Dtos;

public record AcademicLevelSalaryRatioDto(
    Guid Id,
    string? AcademicLevel, 
    decimal? FeeMultiplier
);
