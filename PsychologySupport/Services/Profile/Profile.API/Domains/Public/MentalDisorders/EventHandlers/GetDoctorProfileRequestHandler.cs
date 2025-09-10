using BuildingBlocks.Messaging.Events.Queries.Profile;
using Profile.API.Models.Public;

namespace Profile.API.Domains.Public.MentalDisorders.EventHandlers;

public class GetDoctorProfileRequestHandler(ProfileDbContext dbContext) : IConsumer<GetDoctorProfileRequest>
{
    public async Task Consume(ConsumeContext<GetDoctorProfileRequest> context)
    {
        DoctorProfile? doctorProfile = null;

        if (context.Message.UserId is not null)
        {
            doctorProfile = await dbContext.DoctorProfiles.FirstOrDefaultAsync(x => x.UserId == context.Message.UserId);

            if (doctorProfile is null)
            {
                await context.RespondAsync(new GetDoctorProfileResponse(false, Guid.Empty, default, UserGender.Else, default,
                    default,
                    default, Guid.Empty));
                return;
            }

            await context.RespondAsync(doctorProfile.Adapt<GetDoctorProfileResponse>() with
            {
                Address = doctorProfile.ContactInfo.Address,
                PhoneNumber = doctorProfile.ContactInfo.PhoneNumber,
                Email = doctorProfile.ContactInfo.Email,
                DoctorExists = true,
                UserId = doctorProfile.UserId
            });
            return;
        }

        doctorProfile = await dbContext.DoctorProfiles.AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == context.Message.DoctorId);

        if (doctorProfile is null)
        {
            await context.RespondAsync(new GetDoctorProfileResponse(false, Guid.Empty, default, UserGender.Else, default, default,
                default, Guid.Empty));
            return;
        }

        await context.RespondAsync(doctorProfile.Adapt<GetDoctorProfileResponse>() with
        {
            FullName = doctorProfile.FullName,
            Address = doctorProfile.ContactInfo.Address,
            PhoneNumber = doctorProfile.ContactInfo.PhoneNumber,
            Email = doctorProfile.ContactInfo.Email,
            DoctorExists = true,
            UserId = doctorProfile.UserId
        });
    }
}