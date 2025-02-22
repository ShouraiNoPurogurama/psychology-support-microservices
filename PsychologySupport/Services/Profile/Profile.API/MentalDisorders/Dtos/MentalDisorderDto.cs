namespace Profile.API.MentalDisorders.Dtos;

public record MentalDisorderDto(
    Guid Id,
    string Name,
    string Description
);
