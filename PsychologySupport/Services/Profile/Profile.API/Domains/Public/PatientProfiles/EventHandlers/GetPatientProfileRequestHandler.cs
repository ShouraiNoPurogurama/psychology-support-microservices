using BuildingBlocks.Messaging.Events.Queries.Profile;
using Microsoft.EntityFrameworkCore;
using Profile.API.Data.Pii;

namespace Profile.API.Domains.Public.PatientProfiles.EventHandlers
{
    public class GetPatientProfileRequestHandler : IConsumer<GetPatientProfileRequest>
    {
        private readonly PiiDbContext _piiDbContext;

        public GetPatientProfileRequestHandler(PiiDbContext piiDbContext)
        {
            _piiDbContext = piiDbContext;
        }

        public async Task Consume(ConsumeContext<GetPatientProfileRequest> context)
        {
            var request = context.Message;
            var ct = context.CancellationToken;

            try
            {
                // Truy vấn PatientOwnerMap kèm PersonProfile
                var query = _piiDbContext.PatientOwnerMaps
                    .Include(p => p.PersonProfile)
                    .AsQueryable();

                if (request.PatientId != Guid.Empty)
                {
                    query = query.Where(p => p.PatientProfileId == request.PatientId);
                }
                else if (request.UserId.HasValue)
                {
                    query = query.Where(p => p.SubjectRef == request.UserId.Value);
                }

                var patientMap = await query.AsNoTracking().FirstOrDefaultAsync(ct);

                if (patientMap is null)
                {
                    await context.RespondAsync(new GetPatientProfileResponse(
                        PatientExists: false,
                        Id: Guid.Empty,
                        FullName: string.Empty,
                        PhoneNumber: string.Empty,
                        Email: string.Empty,
                        UserId: Guid.Empty
                    ));
                    return;
                }

                var profile = patientMap.PersonProfile;

                await context.RespondAsync(new GetPatientProfileResponse(
                    PatientExists: true,
                    Id: patientMap.PatientProfileId,
                    FullName: profile.FullName.Value,
                    PhoneNumber: profile.ContactInfo.PhoneNumber ?? string.Empty,
                    Email: profile.ContactInfo.Email,
                    UserId: profile.UserId
                ));
            }
            catch
            {
                // Trả về mặc định nếu có lỗi
                await context.RespondAsync(new GetPatientProfileResponse(
                    PatientExists: false,
                    Id: Guid.Empty,
                    FullName: string.Empty,
                    PhoneNumber: string.Empty,
                    Email: string.Empty,
                    UserId: Guid.Empty
                ));
            }
        }
    }
}
