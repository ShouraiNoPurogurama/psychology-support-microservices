using System.Security.Claims;
using BuildingBlocks.Enums;
using UserMemory.API.Shared.Services.Contracts;

namespace UserMemory.API.Shared.Services;

public class CurrentCurrentUserSubscriptionAccessor : ICurrentUserSubscriptionAccessor
{
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<CurrentCurrentUserSubscriptionAccessor> _logger;

    public CurrentCurrentUserSubscriptionAccessor(IHttpContextAccessor http,
        ILogger<CurrentCurrentUserSubscriptionAccessor> logger)
    {
        _http = http;
        _logger = logger;
    }

    public SubscriptionTier GetCurrentTier()
    {
        var planName = _http.HttpContext?.User.FindFirstValue("SubscriptionPlanName");

        if (string.IsNullOrEmpty(planName))
        {
            _logger.LogWarning("Missing subscription plan name, defaulting to Free.");
            return SubscriptionTier.Free;
        }

        var result = planName switch
        {
            "Free Plan" => SubscriptionTier.Free,

            _ when planName.Contains("Free Trial") => SubscriptionTier.FreeTrial,
            _ when planName.Contains("Premium Plan") => SubscriptionTier.Premium,

            // Bất kỳ gói nào không map được
            _ => LogAndReturnFree(planName)
        };

        return result;
    }

    SubscriptionTier LogAndReturnFree(string planName)
    {
        _logger.LogWarning("Unmapped subscription plan: {PlanName}. Defaulting to Free.", planName);
        return SubscriptionTier.Free;
    }
}