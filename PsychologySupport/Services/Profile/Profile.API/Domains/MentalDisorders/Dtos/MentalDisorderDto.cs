namespace Profile.API.Domains.MentalDisorders.Dtos;

public record MentalDisorderDto(
    Guid Id,
    string Name,
    string Description
);