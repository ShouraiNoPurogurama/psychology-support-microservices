using LifeStyles.API.Events; 
using BuildingBlocks.Messaging.Events.Profile; 

namespace Profile.API.EventHandlers
{
    public class CheckPatientProfileExistenceEventHandler : IConsumer<CheckPatientProfileExistenceIntegrationEvent>
    {
        private readonly ProfileDbContext _context;

        public CheckPatientProfileExistenceEventHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<CheckPatientProfileExistenceIntegrationEvent> context)
        {
            var message = context.Message;

            var patientProfile = await _context.PatientProfiles
                .FindAsync(message.PatientProfileId);

            bool exists = patientProfile != null;

            await context.RespondAsync(new CheckPatientProfileExistenceResponseEvent(message.PatientProfileId, exists));
        }
    }
}
