namespace Profile.API.MentalDisorders.Dtos;

public record SpecificMentalDisorderDto(
    Guid Id,
    string MentalDisorderName,
    string Name,
    string Description
);