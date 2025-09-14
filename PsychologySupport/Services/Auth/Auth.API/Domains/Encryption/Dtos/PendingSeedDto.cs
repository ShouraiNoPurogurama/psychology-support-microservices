
namespace Auth.API.Domains.Encryption.Dtos;

public record PendingSeedDto(
    string FullName,
    string Email,
    string? PhoneNumber
);