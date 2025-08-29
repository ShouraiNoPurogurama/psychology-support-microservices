using BuildingBlocks.Enums;

namespace Auth.API.Domains.Authentication.Dtos.Requests;

public record RegisterRequest(
    string FullName,
    UserGender Gender,
    string Email,
    string PhoneNumber,
    string Password,
    string ConfirmPassword,
    DateOnly BirthDate
    );