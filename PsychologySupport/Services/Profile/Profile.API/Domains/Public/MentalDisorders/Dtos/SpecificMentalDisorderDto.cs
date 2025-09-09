namespace Profile.API.Domains.MentalDisorders.Dtos;

public record SpecificMentalDisorderDto(
    Guid Id,
    string MentalDisorderName,
    string Name,
    string Description
);