using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using Profile.API.Data.Public;

namespace Profile.API.Domains.Public.PatientProfiles.EventHandlers
{
    /// <summary>
    /// Handler xử lý event kiểm tra sự tồn tại của PatientProfile theo Id.
    /// </summary>
    public class PatientProfileExistenceRequestHandler : IConsumer<PatientProfileExistenceRequest>
    {
        private readonly ProfileDbContext _context;

        public PatientProfileExistenceRequestHandler(ProfileDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<PatientProfileExistenceRequest> context)
        {
            var profileId = context.Message.PatientProfileId;

            var isExist = await _context.PatientProfiles
                .AnyAsync(x => x.Id == profileId);

            await context.RespondAsync(new PatientProfileExistenceResponse(isExist));
        }
    }
}
