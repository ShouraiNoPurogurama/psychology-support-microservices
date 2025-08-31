using BuildingBlocks.CQRS;
using Mapster;
using Subscription.API.Data;
using Subscription.API.Exceptions;
using Subscription.API.UserSubscriptions.Dtos;

namespace Subscription.API.UserSubscriptions.Features.v1.UpdateUserSubscription;

//TODO Only status of subscription can be updated
public record UpdateUserSubscriptionCommand(UserSubscriptionDto UserSubscription) : ICommand<UpdateUserSubscriptionResult>;

public record UpdateUserSubscriptionResult(bool IsSuccess);

public class UpdateUserSubscriptionHandler(SubscriptionDbContext context)
    : ICommandHandler<UpdateUserSubscriptionCommand, UpdateUserSubscriptionResult>
{
    public async Task<UpdateUserSubscriptionResult> Handle(UpdateUserSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        var existingSubscription = await context.UserSubscriptions.FindAsync(request.UserSubscription.Id, cancellationToken)
                                   ?? throw new SubscriptionNotFoundException("User Subscription", request.UserSubscription.Id);

        existingSubscription = request.UserSubscription.Adapt(existingSubscription);

        context.Update(existingSubscription);

        var result = await context.SaveChangesAsync(cancellationToken) > 0;

        return new UpdateUserSubscriptionResult(result);
    }
}