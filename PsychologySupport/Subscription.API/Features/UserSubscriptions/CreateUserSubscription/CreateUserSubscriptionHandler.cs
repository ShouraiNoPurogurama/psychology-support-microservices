using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Models;

namespace Subscription.API.Features.UserSubscriptions.CreateUserSubscription;

public record CreateUserSubscriptionCommand(UserSubscription UserSubscription) : ICommand<CreateUserSubscriptionResult>;

public record CreateUserSubscriptionResult(Guid Id);

public class CreateUserSubscriptionHandler : ICommandHandler<CreateUserSubscriptionCommand, CreateUserSubscriptionResult>
{
    private readonly SubscriptionDbContext _context;

    public CreateUserSubscriptionHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<CreateUserSubscriptionResult> Handle(CreateUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        _context.UserSubscriptions.Add(request.UserSubscription);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateUserSubscriptionResult(request.UserSubscription.Id);
    }
}