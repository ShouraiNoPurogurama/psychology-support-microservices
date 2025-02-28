using BuildingBlocks.Messaging.Events.Profile; 

namespace Profile.API.EventHandlers
{
    public class CheckPatientProfileExistenceEventHandler : IConsumer<PatientProfileExistenceRequest>
    {
        private readonly ProfileDbContext _context;

        public CheckPatientProfileExistenceEventHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<PatientProfileExistenceRequest> context)
        {
            var message = context.Message;

            var patientProfile = await _context.PatientProfiles
                .FindAsync(message.PatientProfileId);

            await context.RespondAsync(new PatientProfileExistenceResponse(patientProfile is not null));
        }
    }
}
