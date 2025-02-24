using MassTransit;
using Auth.API.Data;
using Profile.API.PatientProfiles.Events;
using Auth.API.Data.Enums;
using Auth.API.Models;

namespace Auth.API.EventHandlers;

public class PatientProfileCreatedEventHandler : IConsumer<PatientProfileCreatedEvent>
{
    private readonly AuthDbContext _context;

    public PatientProfileCreatedEventHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<PatientProfileCreatedEvent> context)
    {
        var message = context.Message;

        var user = await _context.Users.FindAsync(message.UserId);
        if (user != null)
        {
            user.Gender = message.Gender == "Male" ? UserGender.Male : UserGender.Female;
            user.Email = message.Email;
            user.PhoneNumber = message.PhoneNumber;

            await _context.SaveChangesAsync();
        }
    }
}
