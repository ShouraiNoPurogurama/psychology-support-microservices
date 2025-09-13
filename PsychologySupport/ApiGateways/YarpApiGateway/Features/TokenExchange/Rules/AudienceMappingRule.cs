namespace YarpApiGateway.Features.TokenExchange.Rules;

public class AudienceMappingRule
{
    public required IEnumerable<string> Keywords { get; init; }
    public required string ClaimType { get; init; }
    public required Func<string, Task<string?>> LookupFunction { get; init; }
}