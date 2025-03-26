using Auth.API.Models;
using BuildingBlocks.Dtos;
using BuildingBlocks.Messaging.Events.Auth;
using Mapster;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Auth.API.EventHandlers;

public class GetAllUsersDataRequestHandler(UserManager<User> userManager) : IConsumer<GetAllUsersDataRequest>
{
    public async Task Consume(ConsumeContext<GetAllUsersDataRequest> context)
    {
        var message = context.Message;
        var users = userManager.Users;

        // if (context.Message.Role is not null)
        // {
        //     List<User> matchingUsers = await userManager
        //         .GetUsersInRoleAsync(context.Message.Role)
        //         .ContinueWith(u => u.Result.ToList());
        //
        //     await context.RespondAsync(new GetOnlineUsersDataResponse(matchingUsers.Adapt<List<UserDto>>()));
        //     return;
        // }

        if (message.UserIds is not null)
        {
            if (message.UserIds.Count == 0) await context.RespondAsync(new GetOnlineUsersDataResponse(new List<UserDto>()));
            var matchingUsers = await userManager.Users
                .Where(u => message.UserIds.Contains(u.Id))
                .ProjectToType<UserDto>()
                .ToListAsync();

            await context.RespondAsync(new GetOnlineUsersDataResponse(matchingUsers));
            return;
        }

        var result = await users.ProjectToType<UserDto>().ToListAsync();

        await context.RespondAsync(new GetOnlineUsersDataResponse(result));
    }
}