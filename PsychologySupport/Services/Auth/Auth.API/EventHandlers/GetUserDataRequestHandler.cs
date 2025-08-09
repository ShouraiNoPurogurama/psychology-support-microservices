using System.Linq.Expressions;
using Auth.API.Models;
using BuildingBlocks.Messaging.Events.Auth;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.EventHandlers;

public class GetUserDataRequestHandler(UserManager<User> userManager) : IConsumer<GetUserDataRequest>
{
    public async Task Consume(ConsumeContext<GetUserDataRequest> context)
    {
        Expression<Func<User, bool>> predicate = u =>
            u.Id.Equals(context.Message.UserId) || u.Email!.Equals(context.Message.UserEmail);

        var user = await userManager.Users
            .Include(u => u.Devices)
            .FirstOrDefaultAsync(predicate);

        if (user is null)
        {
            await context.RespondAsync(new GetUserDataResponse(Guid.Empty, default, default, []));
            return;
        }

        await context.RespondAsync(new GetUserDataResponse(user.Id, user.UserName!, user.Email!,
            user.Devices
                .Where(d => !string.IsNullOrEmpty(d.DeviceToken))
                .Select(d => d.DeviceToken!)));
    }
}