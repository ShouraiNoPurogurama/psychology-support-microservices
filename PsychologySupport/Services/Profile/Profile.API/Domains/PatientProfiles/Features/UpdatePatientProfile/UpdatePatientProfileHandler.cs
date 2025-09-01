using BuildingBlocks.Data.Common;
using Profile.API.Data.Pii;
using Profile.API.Data.Public;
using Profile.API.Domains.PatientProfiles.Dtos;
using Profile.API.Domains.PatientProfiles.Exceptions;

namespace Profile.API.Domains.PatientProfiles.Features.UpdatePatientProfile;

public record UpdatePatientProfileCommand(Guid AliasId, UpdatePatientProfileDto PatientProfileUpdate)
    : ICommand<UpdatePatientProfileResult>;

public record UpdatePatientProfileResult(Guid AliasId);

public class UpdatePatientProfileHandler : ICommandHandler<UpdatePatientProfileCommand, UpdatePatientProfileResult>
{
    private readonly ProfileDbContext _context;
    private readonly PiiDbContext _piiDbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdatePatientProfileHandler(ProfileDbContext context, IPublishEndpoint publishEndpoint, PiiDbContext piiDbContext)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _piiDbContext = piiDbContext;
    }

    public async Task<UpdatePatientProfileResult> Handle(UpdatePatientProfileCommand request, CancellationToken cancellationToken)
    {
        var patientProfile = await _context.PatientProfiles.FindAsync([request.AliasId], cancellationToken)
                             ?? throw new ProfileNotFoundException(request.AliasId);

        var dto = request.PatientProfileUpdate;

        var pii = await _piiDbContext.AliasOwnerMaps
                      .Include(a => a.PersonProfile)
                      .FirstOrDefaultAsync(a => a.AliasId == request.AliasId, cancellationToken: cancellationToken)
                  ?? throw new ProfileNotFoundException(request.AliasId);

        var piiProfile = pii.PersonProfile;
        
        var newContactInfo = ContactInfo.UpdateWithFallback(piiProfile.ContactInfo, dto.ContactInfo?.Address,
            dto.ContactInfo?.Email, dto.ContactInfo?.PhoneNumber);

        patientProfile.Update(
            dto.Allergies ?? patientProfile.Allergies,
            dto.PersonalityTraits ?? patientProfile.PersonalityTraits,
            dto.JobId ?? patientProfile.JobId
        );
        
        piiProfile.Update(
            dto.FullName,
            dto.Gender ?? piiProfile.Gender,
            dto.BirthDate ?? piiProfile.BirthDate,
            newContactInfo
            );

        await _context.SaveChangesAsync(cancellationToken);

        var patientProfileUpdatedEvent = new PatientProfileUpdatedIntegrationEvent(
            // patientProfile.UserId,
            piiProfile.FullName!,
            piiProfile.Gender,
            piiProfile.ContactInfo.Email,
            piiProfile.ContactInfo.PhoneNumber!
        );

        await _publishEndpoint.Publish(patientProfileUpdatedEvent, cancellationToken);

        return new UpdatePatientProfileResult(patientProfile.Id);
    }
}