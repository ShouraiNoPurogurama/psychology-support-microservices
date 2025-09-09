using BuildingBlocks.Data.Common;
using BuildingBlocks.Messaging.Events.Queries.Profile;
using Profile.API.Data.Pii;

namespace Profile.API.Domains.Public.PatientProfiles.EventHandlers;

public class GetPatientProfileHandler(ProfileDbContext dbContext, PiiDbContext piiDbContext)
    : IConsumer<GetPatientProfileRequest>
{
    public async Task Consume(ConsumeContext<GetPatientProfileRequest> context)
    {
        // var message = context.Message;
        // PatientProfile? patientProfile = null;
        //
        //TODO tí quay lại sửa
        
        // if (message.UserId is not null)
        // {
        //     patientProfile = await dbContext.PatientProfiles
        //         .AsNoTracking()
        //         .FirstOrDefaultAsync(x => x.UserId == message.UserId, context.CancellationToken);
        // }
        // else if (message.PatientId != Guid.Empty)
        // {
        //     patientProfile = await dbContext.PatientProfiles
        //         .AsNoTracking()
        //         .FirstOrDefaultAsync(p => p.Id == message.PatientId, context.CancellationToken);
        // }
        //
        // if (patientProfile is null)
        // {
        //     await context.RespondAsync(BuildNotFoundResponse());
        //     return;
        // }
        //
        // var contactInfo = await GetContactInfo(patientProfile.UserId, context.CancellationToken);
        //
        // var response = patientProfile.Adapt<GetPatientProfileResponse>() with
        // {
        //     Email = contactInfo.Email,
        //     Address = contactInfo.Address,
        //     PhoneNumber = contactInfo.PhoneNumber,
        //     PatientExists = true,
        //     UserId = patientProfile.UserId
        // };
        //
        // await context.RespondAsync(response);
    }

    private static GetPatientProfileResponse BuildNotFoundResponse() =>
        new(false, Guid.Empty, string.Empty, UserGender.Else,
            string.Empty, string.Empty, string.Empty, string.Empty,
            string.Empty, Guid.Empty, false);

    private async Task<ContactInfo> GetContactInfo(Guid userId, CancellationToken ct)
    {
        var person = await piiDbContext.AliasOwnerMaps
            .AsNoTracking()
            .Include(p => p.PersonProfile)
            .Select(a => a.PersonProfile)
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

        return person?.ContactInfo ?? new ContactInfo();
    }
}