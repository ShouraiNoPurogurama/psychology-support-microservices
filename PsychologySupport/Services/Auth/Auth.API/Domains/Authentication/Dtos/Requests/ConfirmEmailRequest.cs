namespace Auth.API.Domains.Authentication.Dtos.Requests;

public record ConfirmEmailRequest(
    string Token,
    string Email
);