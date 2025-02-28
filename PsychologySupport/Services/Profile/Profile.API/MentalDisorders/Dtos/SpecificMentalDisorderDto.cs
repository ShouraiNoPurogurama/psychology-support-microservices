namespace Profile.API.MentalDisorders.Dtos;

public record SpecificMentalDisorderDto(
    Guid Id,
    Guid MentalDisorderId,
    string Name,
    string Description
);