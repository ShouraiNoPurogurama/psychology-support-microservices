using BuildingBlocks.Enums;

namespace Auth.API.Dtos.Requests;

public record RegisterRequest(
    string FullName,
    UserGender Gender,
    string Email,
    string PhoneNumber,
    string Password,
    string ConfirmPassword,
    DateOnly BirthDate
    );