using BuildingBlocks.Messaging.Events.Profile;

namespace Profile.API.EventHandlers;

public class PatientProfileExistenceByEmailHandler : IConsumer<PatientProfileExistenceByEmailRequest>
{
    private readonly ProfileDbContext _context;

    public PatientProfileExistenceByEmailHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<PatientProfileExistenceByEmailRequest> context)
    {
        var email = context.Message.email;

        var isExist = await _context.PatientProfiles
            .AnyAsync(x => x.ContactInfo.Email == email);

        await context.RespondAsync(new PatientProfileExistenceByEmailResponse(isExist));
    }
}