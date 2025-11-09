using System.Security.Claims;
using BuildingBlocks.Exceptions;

namespace ChatBox.API.Shared.Subscription;

public class CurrentCurrentUserSubscriptionAccessor : ICurrentUserSubscriptionAccessor
{
    private readonly IHttpContextAccessor _http;
    private readonly ILogger<CurrentCurrentUserSubscriptionAccessor> _logger;

    public CurrentCurrentUserSubscriptionAccessor(IHttpContextAccessor http, ILogger<CurrentCurrentUserSubscriptionAccessor> logger)
    {
        _http = http;
        _logger = logger;
    }

    public bool IsFreeTier()
    {
        var subscriptionPlan = _http.HttpContext?.User.FindFirstValue("SubscriptionPlanName");

        if (string.IsNullOrEmpty(subscriptionPlan))
        {
            _logger.LogWarning("Missing or invalid subscription plan name.");
            return false;
        }
        
        if (subscriptionPlan != "Free Plan")
        {
            return false;
        }

        return true;
    }

    public string GetCurrentSubscription()
    {
        var subscriptionPlan = _http.HttpContext?.User.FindFirstValue("SubscriptionPlanName");

        if (string.IsNullOrEmpty(subscriptionPlan))
        {
            throw new ForbiddenException();
        }
        
        return subscriptionPlan;
    }
}