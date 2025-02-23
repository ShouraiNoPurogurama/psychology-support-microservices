using MassTransit;
using Auth.API.Data;
using Profile.API.PatientProfiles.Events;
using Auth.API.Data.Enums;

namespace Auth.API.EventHandlers;

public class PatientProfileUpdatedEventHandler : IConsumer<PatientProfileUpdatedEvent>
{
    private readonly AuthDbContext _context;

    public PatientProfileUpdatedEventHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<PatientProfileUpdatedEvent> context)
    {
        var message = context.Message;

        var user = await _context.Users.FindAsync(message.UserId);
        if (user != null)
        {
            user.FullName = message.FullName;
            user.Gender = message.Gender == "Male" ? UserGender.Male : UserGender.Female;
            user.Email = message.Email;
            user.PhoneNumber = message.PhoneNumber;

            await _context.SaveChangesAsync();
        }
    }
}
