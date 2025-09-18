using Profile.API.Models.Public;

namespace Profile.API.Domains.Public.PatientProfiles.Features.SeedPatientProfile;

public record SeedPatientProfileCommand(
    Guid ProfileId
) : ICommand<SeedPatientProfileResult>;

public record SeedPatientProfileResult(bool IsSuccess);

public class SeedPatientProfileHandler(ProfileDbContext dbContext, ILogger<SeedPatientProfileHandler> logger)
    : ICommandHandler<SeedPatientProfileCommand, SeedPatientProfileResult>
{
    public async Task<SeedPatientProfileResult> Handle(
        SeedPatientProfileCommand request,
        CancellationToken cancellationToken)
    {
        var profileId = request.ProfileId;

        var existedProfile = await dbContext.PatientProfiles
            .AsNoTracking()
            .AnyAsync(p => p.Id == profileId, cancellationToken);

        if (existedProfile)
        {
            logger.LogWarning($"Profile with id {profileId} already exists, cannot create duplicate.");

            return new SeedPatientProfileResult(false);
        }
        
        var profile = PatientProfile.CreateSeed(
            seedProfileId: profileId
        );

        dbContext.PatientProfiles.Add(profile);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new SeedPatientProfileResult(true);
    }
}