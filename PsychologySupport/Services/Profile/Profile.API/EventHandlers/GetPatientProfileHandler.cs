using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Profile;
using Mapster;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.EventHandlers;

public class GetPatientProfileHandler(ProfileDbContext dbContext) : IConsumer<GetPatientProfileRequest>
{
    public async Task Consume(ConsumeContext<GetPatientProfileRequest> context)
    {
        var patientProfile = await dbContext.PatientProfiles.AsNoTracking()
            .Include(patientProfile => patientProfile.ContactInfo)
            .FirstOrDefaultAsync(p => p.Id == context.Message.PatientId); 
        
        if (patientProfile is null)
        {
            await context.RespondAsync(new GetPatientProfileResponse( false,Guid.Empty,  string.Empty, UserGender.Else, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty));
            return;
        }
        
        if (context.Message.UserId is not null)
        {
            patientProfile = patientProfile.UserId == context.Message.UserId ? patientProfile : null;
        }
        
        if (patientProfile is null)
        {
            await context.RespondAsync(new GetPatientProfileResponse( false,Guid.Empty,  string.Empty, UserGender.Else, string.Empty, string.Empty, string.Empty, string.Empty, String.Empty));
            return;
        }

        await context.RespondAsync(patientProfile.Adapt<GetPatientProfileResponse>() with
        {
            Email = patientProfile.ContactInfo.Email,
            Address = patientProfile.ContactInfo.Address,
            PhoneNumber = patientProfile.ContactInfo.PhoneNumber,
            PatientExists = true
        });
    }
}