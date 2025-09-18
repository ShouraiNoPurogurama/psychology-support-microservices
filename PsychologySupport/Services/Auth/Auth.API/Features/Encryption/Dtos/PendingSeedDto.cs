
namespace Auth.API.Features.Encryption.Dtos;

public record PendingSeedDto(
    string FullName,
    string Email,
    string? PhoneNumber
);