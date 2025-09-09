using BuildingBlocks.Messaging.Events.Queries.Profile;
using Profile.API.Data.Pii;

namespace Profile.API.Domains.Public.PatientProfiles.EventHandlers;

public class PatientProfileExistenceByEmailHandler : IConsumer<PatientProfileExistenceByEmailRequest>
{
    private readonly PiiDbContext _context;

    public PatientProfileExistenceByEmailHandler(PiiDbContext context)
    {
        _context = context;
    }

    public async Task Consume(ConsumeContext<PatientProfileExistenceByEmailRequest> context)
    {
        var email = context.Message.email;

        var isExist = await _context.PersonProfiles
            .AnyAsync(x => x.ContactInfo.Email == email);

        await context.RespondAsync(new PatientProfileExistenceByEmailResponse(isExist));
    }
}