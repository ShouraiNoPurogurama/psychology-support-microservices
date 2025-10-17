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
                Keywords = new[] { "alias", "post", "feed", "notification" },
                ClaimType = "aliasId",
                LookupFunction = (subRef) => _piiLookupService.ResolveAliasIdBySubjectRefAsync(subRef)
            },
            new AudienceMappingRule
            {
                Keywords = new[] { "profile" ,"subscription"},
                ClaimType = "patientId",
                LookupFunction = (subRef) => _piiLookupService.ResolvePatientIdBySubjectRefAsync(subRef)
            },
             new AudienceMappingRule
            {
                Keywords = new[] { "chatbox" },
                ClaimType = "userId",
                LookupFunction = (subRef) => _piiLookupService.ResolveUserIdBySubjectRefAsync(subRef)
            }
        };
    }
}