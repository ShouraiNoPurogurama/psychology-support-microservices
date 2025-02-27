using MassTransit;
using Auth.API.Data;
using Auth.API.Models;
using BuildingBlocks.Messaging.Events.Auth;
using Microsoft.AspNetCore.Identity;

namespace Auth.API.EventHandlers;

public class DoctorProfileCreatedEventHandler : IConsumer<DoctorProfileCreatedIntegrationEvent>
{
    private readonly AuthDbContext _context;
    private readonly UserManager<User> _userManager;

    public DoctorProfileCreatedEventHandler(AuthDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task Consume(ConsumeContext<DoctorProfileCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        var user = await _context.Users.FindAsync(message.UserId);
        if (user != null)
        {
            user.FullName = message.FullName;
            user.Gender = message.Gender;
            await _userManager.SetEmailAsync(user,message.Email);
            await _userManager.SetPhoneNumberAsync(user,message.PhoneNumber);
            
            await _context.SaveChangesAsync();
        }
    }
}
