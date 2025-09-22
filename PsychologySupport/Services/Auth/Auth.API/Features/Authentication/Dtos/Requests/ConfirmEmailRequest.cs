namespace Auth.API.Features.Authentication.Dtos.Requests;

public record ConfirmEmailRequest(
    string Token,
    string Email
);