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
        var user = await userManager.FindByIdAsync(context.Message.UserId);
        
        if (user is null)
        {
            await context.RespondAsync(new GetUserDataResponse(Guid.Empty, default, default));
            return;
        }
        
        await context.RespondAsync(new GetUserDataResponse(user.Id, user.UserName, user.Email));
    }
}