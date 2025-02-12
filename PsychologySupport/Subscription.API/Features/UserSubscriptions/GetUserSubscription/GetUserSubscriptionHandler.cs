using BuildingBlocks.CQRS;
using FluentValidation;
using Subscription.API.Data;
using Subscription.API.Exceptions;
using Subscription.API.Models;

namespace Subscription.API.Features.UserSubscriptions.GetUserSubscription;

public record GetUserSubscriptionQuery(Guid Id) : IQuery<GetUserSubscriptionResult>;

public record GetUserSubscriptionResult(UserSubscription UserSubscription);

public class GetUserSubscriptionQueryValidator : AbstractValidator<GetUserSubscriptionQuery>
{
    public GetUserSubscriptionQueryValidator()
    {
        RuleFor(q => q.Id).NotEmpty().WithMessage("Id subscription không được để trống");
    }
}

public class GetUserSubscriptionHandler : IQueryHandler<GetUserSubscriptionQuery, GetUserSubscriptionResult>
{
    private readonly SubscriptionDbContext _context;

    public GetUserSubscriptionHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<GetUserSubscriptionResult> Handle(GetUserSubscriptionQuery query, CancellationToken cancellationToken)
    {
        var userSubscription = await _context.UserSubscriptions.FindAsync(query.Id)
                                ?? throw new SubscriptionNotFoundException(query.Id.ToString());

        return new GetUserSubscriptionResult(userSubscription);
    }
}