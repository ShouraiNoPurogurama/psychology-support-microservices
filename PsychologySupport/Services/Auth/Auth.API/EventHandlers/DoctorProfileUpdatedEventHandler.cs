using Auth.API.Data;
using Auth.API.Models;
using BuildingBlocks.Messaging.Events.Auth;
using MassTransit;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.EventHandlers;

public class DoctorProfileUpdatedEventHandler : IConsumer<DoctorProfileUpdatedIntegrationEvent>
{
    private readonly AuthDbContext _context;
    private readonly UserManager<User> _userManager;

    public DoctorProfileUpdatedEventHandler(AuthDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task Consume(ConsumeContext<DoctorProfileUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        var user = await _context.Users.FindAsync(message.UserId);
        if (user != null)
        {
            user.FullName = message.FullName;
            user.Gender = message.Gender;
            await _userManager.SetEmailAsync(user, message.Email);
            await _userManager.SetPhoneNumberAsync(user, message.PhoneNumber);
            user.EmailConfirmed = true;
            user.PhoneNumberConfirmed = true;
            await _context.SaveChangesAsync();
        }
    }
}