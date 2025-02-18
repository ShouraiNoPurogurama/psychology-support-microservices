using Auth.API.Data.Enums;

namespace Auth.API.Dtos.Requests;

public record RegisterRequest(string FullName, UserGender Gender, string Email, string PhoneNumber, string Password, string ConfirmPassword);