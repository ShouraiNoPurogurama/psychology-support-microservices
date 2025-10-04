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
            new AudienceMappingRule
            {
                Keywords = new[] { "alias", "post", "feed" },
                ClaimType = "aliasId",
                LookupFunction = (subRef) => _piiLookupService.ResolveAliasIdBySubjectRefAsync(subRef)
            },
            new AudienceMappingRule
            {
                Keywords = new[] { "profile" ,"subscription", "chatbox" },
                ClaimType = "patientId",
                LookupFunction = (subRef) => _piiLookupService.ResolvePatientIdBySubjectRefAsync(subRef)
            }
        };
    }
}