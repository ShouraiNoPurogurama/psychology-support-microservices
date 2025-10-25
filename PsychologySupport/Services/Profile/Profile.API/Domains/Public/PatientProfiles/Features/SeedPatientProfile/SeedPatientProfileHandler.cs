using Profile.API.Models.Public;

namespace Profile.API.Domains.Public.PatientProfiles.Features.SeedPatientProfile;

public record SeedPatientProfileCommand(
    Guid ProfileId,
    string? ReferralCode = null
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
        var referralCode = request.ReferralCode;

        var existedProfile = await dbContext.PatientProfiles
            .AsNoTracking()
            .AnyAsync(p => p.Id == profileId, cancellationToken);

        if (existedProfile)
        {
            logger.LogWarning($"Profile with id {profileId} already exists, cannot create duplicate.");

            return new SeedPatientProfileResult(false);
        }

        if (!string.IsNullOrWhiteSpace(referralCode))
        {
            var existedInPatient = await dbContext.PatientProfiles
                .AsNoTracking()
                .AnyAsync(p => p.ReferralCode == referralCode, cancellationToken);

            var existedInAffiliate = await dbContext.AffiliateProfiles
                .AsNoTracking()
                .AnyAsync(a => a.ReferralCode == referralCode, cancellationToken);

            if (existedInPatient || existedInAffiliate)
            {
                logger.LogWarning($"Referral code '{referralCode}' already exists in PatientProfile or AffiliateProfile, cannot create duplicate.");

                return new SeedPatientProfileResult(false);
            }
        }

        var profile = PatientProfile.CreateSeed(
            seedProfileId: profileId,
            referralCode: referralCode 
        );

        dbContext.PatientProfiles.Add(profile);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new SeedPatientProfileResult(true);
    }
}