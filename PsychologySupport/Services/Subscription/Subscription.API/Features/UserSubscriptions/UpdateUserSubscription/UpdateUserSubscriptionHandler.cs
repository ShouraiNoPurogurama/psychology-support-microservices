using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Dtos;
using Mapster;
using Subscription.API.Exceptions;

namespace Subscription.API.Features.UserSubscriptions.UpdateUserSubscription;

public record UpdateUserSubscriptionCommand(UserSubscriptionDto UserSubscription) : ICommand<UpdateUserSubscriptionResult>;

public record UpdateUserSubscriptionResult(bool IsSuccess);

public class UpdateUserSubscriptionHandler : ICommandHandler<UpdateUserSubscriptionCommand, UpdateUserSubscriptionResult>
{
    private readonly SubscriptionDbContext _context;

    public UpdateUserSubscriptionHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateUserSubscriptionResult> Handle(UpdateUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var existingSubscription = await _context.UserSubscriptions.FindAsync(request.UserSubscription.Id)
                                       ?? throw new SubscriptionNotFoundException("User Subscription", request.UserSubscription.Id);

        existingSubscription = request.UserSubscription.Adapt(existingSubscription);
        existingSubscription.LastModified = DateTime.UtcNow;

        _context.Update(existingSubscription);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        return new UpdateUserSubscriptionResult(result);
    }
}