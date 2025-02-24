using MassTransit;
using Auth.API.Data;
using BuildingBlocks.Messaging.Events.Auth;

namespace Auth.API.EventHandlers;

public class DoctorProfileUpdatedEventHandler : IConsumer<DoctorProfileUpdatedIntegrationEvent>
{
    private readonly AuthDbContext _context;

    public DoctorProfileUpdatedEventHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<DoctorProfileUpdatedIntegrationEvent> context)
    {
        var message = context.Message;

        var user = await _context.Users.FindAsync(message.UserId);
        if (user != null)
        {
            user.Gender = message.Gender;
            user.Email = message.Email;
            user.PhoneNumber = message.PhoneNumber;

            await _context.SaveChangesAsync();
        }
    }
}
