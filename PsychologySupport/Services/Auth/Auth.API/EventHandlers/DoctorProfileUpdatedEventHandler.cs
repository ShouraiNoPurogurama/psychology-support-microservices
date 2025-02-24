using MassTransit;
using Auth.API.Data;
using Profile.API.DoctorProfiles.Events;
using Auth.API.Data.Enums;

namespace Auth.API.EventHandlers;

public class DoctorProfileUpdatedEventHandler : IConsumer<DoctorProfileUpdatedEvent>
{
    private readonly AuthDbContext _context;

    public DoctorProfileUpdatedEventHandler(AuthDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<DoctorProfileUpdatedEvent> context)
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
