using YarpApiGateway.Features.TokenExchange.Contracts;

namespace YarpApiGateway.Features.TokenExchange.Rules;

public class TokenExchangeRuleRegistry
{
    private readonly IPiiLookupService _piiLookupService;

    public TokenExchangeRuleRegistry(IPiiLookupService piiLookupService)
    {
        _piiLookupService = piiLookupService;
    }

    public IEnumerable<AudienceMappingRule> GetRules(string subjectRef)
    {
        return new List<AudienceMappingRule>
        {
            new()
            {
                Keywords = new[] { "alias", "post", "feed", "notification", "chatbox", "user-memory" },
                ClaimType = "aliasId",
                LookupFunction = subRef => _piiLookupService.ResolveAliasIdBySubjectRefAsync(subRef),
                Required = true
            },
            new()
            {
                Keywords = new[] { "profile", "subscription", "test" },
                ClaimType = "patientId",
                LookupFunction = subRef => _piiLookupService.ResolvePatientIdBySubjectRefAsync(subRef),
                Required = true
            },
            //Chatbox: cần CẢ aliasId & userId
            new()
            {
                Keywords = new[] { "chatbox" },
                ClaimType = "userId",
                LookupFunction = subRef => _piiLookupService.ResolveUserIdBySubjectRefAsync(subRef),
                Required = true
            },
            new()
            {
                Keywords = new[] { "chatbox" },
                ClaimType = "patientId",
                LookupFunction = subRef => _piiLookupService.ResolvePatientIdBySubjectRefAsync(subRef),
                Required = true
            }
        };
    }
}