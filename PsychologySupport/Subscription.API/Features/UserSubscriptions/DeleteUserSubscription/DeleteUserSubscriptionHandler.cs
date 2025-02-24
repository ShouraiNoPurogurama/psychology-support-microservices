using BuildingBlocks.CQRS;
using Subscription.API.Data;
using Subscription.API.Exceptions;

namespace Subscription.API.Features.UserSubscriptions.DeleteUserSubscription;

public record DeleteUserSubscriptionCommand(Guid Id) : ICommand<DeleteUserSubscriptionResult>;

public record DeleteUserSubscriptionResult(bool IsSuccess);

public class DeleteUserSubscriptionHandler : ICommandHandler<DeleteUserSubscriptionCommand, DeleteUserSubscriptionResult>
{
    private readonly SubscriptionDbContext _context;

    public DeleteUserSubscriptionHandler(SubscriptionDbContext context)
    {
        _context = context;
    }

    public async Task<DeleteUserSubscriptionResult> Handle(DeleteUserSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var existingUserSubscription = await _context.UserSubscriptions.FindAsync(request.Id)
                                       ?? throw new SubscriptionNotFoundException("User Subscription", request.Id);

        _context.UserSubscriptions.Remove(existingUserSubscription);

        var result = await _context.SaveChangesAsync() > 0;

        return new DeleteUserSubscriptionResult(result);
    }
}