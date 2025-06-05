namespace Auth.API.Dtos.Requests;

public record ConfirmEmailRequest(
    string Token,
    string Email
);