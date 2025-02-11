namespace Pricing.API.Dtos;

public record ExperiencePriceRangeDto(
    Guid Id,
    int? MinYearsOfExperience,
    int? MaxYearsOfExperience,
    decimal? PricePerSession,
    decimal? PricePerMinute
);
