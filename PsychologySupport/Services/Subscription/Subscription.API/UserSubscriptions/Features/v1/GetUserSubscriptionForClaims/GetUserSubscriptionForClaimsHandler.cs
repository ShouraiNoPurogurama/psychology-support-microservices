using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using Subscription.API.Data;

namespace Subscription.API.UserSubscriptions.Features.v1.GetUserSubscriptionForClaims;

public record GetUserSubscriptionForClaimsQuery(Guid PatientId) : IQuery<GetUserSubscriptionForClaimsResult>;

public record GetUserSubscriptionForClaimsResult(string PlanName);

public class GetUserSubscriptionForClaimsHandler(SubscriptionDbContext dbContext) : IQueryHandler<GetUserSubscriptionForClaimsQuery, GetUserSubscriptionForClaimsResult>
{
    public async Task<GetUserSubscriptionForClaimsResult> Handle(GetUserSubscriptionForClaimsQuery request, CancellationToken cancellationToken)
    {
        var userSubscription = await dbContext.UserSubscriptions
            .Include(x => x.ServicePackage)
            .OrderByDescending(x => x.StartDate)
            .FirstOrDefaultAsync(x => x.PatientId == request.PatientId, cancellationToken: cancellationToken);
        
        return userSubscription == null ? new GetUserSubscriptionForClaimsResult("none") : new GetUserSubscriptionForClaimsResult(userSubscription.ServicePackage.Name);
    }
}