using Auth.API.Enums;

namespace Auth.API.Features.Authentication.Dtos.Responses;

public record AliasIssueStatusResponse(AliasIssueStatus Status, bool AliasIssued);