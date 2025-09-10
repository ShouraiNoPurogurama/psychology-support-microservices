namespace Profile.API.Domains.Public.MentalDisorders.Dtos;

public record MentalDisorderDto(
    Guid Id,
    string Name,
    string Description
);