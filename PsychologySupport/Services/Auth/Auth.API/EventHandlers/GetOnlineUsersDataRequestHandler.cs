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
        var users = await userManager.Users.ProjectToType<UserDto>().ToListAsync();

        await context.RespondAsync(new GetOnlineUsersDataResponse(users));
    }
}