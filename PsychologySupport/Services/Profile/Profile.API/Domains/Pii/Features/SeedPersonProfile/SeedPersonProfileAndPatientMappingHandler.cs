using BuildingBlocks.Data.Common;
using BuildingBlocks.Utils;
using Profile.API.Data.Pii;
using Profile.API.Domains.Pii.Dtos;
using Profile.API.Models.Pii;

namespace Profile.API.Domains.Pii.Features.SeedPersonProfile;

public record SeedPersonProfileAndPatientMappingCommand(Guid UserId, PersonSeedDto Seed)
    : ICommand<SeedPersonProfileAndPatientMappingResult>;

public record SeedPersonProfileAndPatientMappingResult(bool IsSuccess);

public class SeedPersonProfileAndPatientMappingHandler(
    PiiDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ILogger<SeedPersonProfileAndPatientMappingHandler> logger)
    : ICommandHandler<SeedPersonProfileAndPatientMappingCommand, SeedPersonProfileAndPatientMappingResult>
{
    public async Task<SeedPersonProfileAndPatientMappingResult> Handle(SeedPersonProfileAndPatientMappingCommand request,
        CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var seed = request.Seed;

        if (userId == Guid.Empty)
        {
            logger.LogError("User Id không được để trống.");
            return new SeedPersonProfileAndPatientMappingResult(false);
        }

        if (seed.PatientProfileId == Guid.Empty)
        {
            logger.LogError("PatientProfileId không được để trống.");
            return new SeedPersonProfileAndPatientMappingResult(false);
        }

        if (seed.SubjectRef == Guid.Empty)
        {
            logger.LogError("SubjectReference không được để trống.");
            return new SeedPersonProfileAndPatientMappingResult(false);
        }

        var subjectRef = seed.SubjectRef;

        ContactInfo contactInfo = ContactInfo.Of(null, seed.Email, seed.PhoneNumber);

        var personProfile = PersonProfile.SeedPending(
            subjectRef: subjectRef,
            userId: userId,
            fullName: seed.FullName,
            contactInfo: contactInfo
        );

        dbContext.PersonProfiles.Add(personProfile);
        
        var patientOwner = PatientOwnerMap.Create(subjectRef, seed.PatientProfileId);
        
        patientOwner.PersonProfile = personProfile;

        dbContext.PatientOwnerMaps.Add(patientOwner);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            
            //Gom vô đại vì lười làm outbox
            
            return new SeedPersonProfileAndPatientMappingResult(true);
        }
        catch (DbUpdateException ex) when (DbUtils.IsUniqueViolation(ex))
        {
            logger.LogDebug(ex, "Unique(user_id) violated; reading winner");

            var dummyProfileWinner = await dbContext.PersonProfiles
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (dummyProfileWinner == null)
            {
                logger.LogError(ex, "Thua race nhưng không tìm thấy winner cho User {UserId}", userId);
                throw; //Lỗi nghiêm trọng, tới đó rồi debug sửa sau :)
            }

            return new SeedPersonProfileAndPatientMappingResult(false);
        }
    }
}