using BuildingBlocks.Messaging.Events.Profile;
using Profile.API.Data.Public;

namespace Profile.API.Domains.MentalDisorders.EventHandlers
{
    public class DoctorProfileExistenceRequestHandler : IConsumer<DoctorProfileExistenceRequest>
    {
        private readonly ProfileDbContext _context;

        public DoctorProfileExistenceRequestHandler(ProfileDbContext context)
        {
            _context = context;
        }
        public async Task Consume(ConsumeContext<DoctorProfileExistenceRequest> context)
        {
            var message = context.Message;

            var doctorProfile = await _context.DoctorProfiles
                .FindAsync(message.DoctorId);

            await context.RespondAsync(new DoctorProfileExistenceResponse(doctorProfile is not null));
        }
    }
}
