using BuildingBlocks.Enums;
using BuildingBlocks.Messaging.Events.Profile;
using Mapster;

namespace Profile.API.EventHandlers;

public class GetDoctorProfileRequestHandler(ProfileDbContext dbContext) : IConsumer<GetDoctorProfileRequest>
{
    public async Task Consume(ConsumeContext<GetDoctorProfileRequest> context)
    {
        var doctorProfile = await dbContext.DoctorProfiles.FindAsync(context.Message.DoctorId);

        if (doctorProfile is null)
        {
            await context.RespondAsync(new GetDoctorProfileResponse(false, default, UserGender.Else, default, default, default));
            return;
        }

        await context.RespondAsync(doctorProfile.Adapt<GetDoctorProfileResponse>() with
        {
            Address = doctorProfile.ContactInfo.Address,
            PhoneNumber = doctorProfile.ContactInfo.PhoneNumber,
            Email = doctorProfile.ContactInfo.Email,
            DoctorExists = true
        });
    }
}