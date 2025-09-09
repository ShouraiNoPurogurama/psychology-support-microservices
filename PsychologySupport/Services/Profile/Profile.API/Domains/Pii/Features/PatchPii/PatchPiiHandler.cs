using BuildingBlocks.Data.Common;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Profile;
using Profile.API.Data.Pii;
using Profile.API.Domains.Pii.Dtos;
using Profile.API.Domains.Public.PatientProfiles.Exceptions;

namespace Profile.API.Domains.Pii.Features.PatchPii;

public record PatchPiiCommand(Guid SubjectRef, UpdatePiiDto Pii)
    : ICommand<PatchPiiResult>;

public record PatchPiiResult(bool IsSuccess);

public class PatchPiiHandler : ICommandHandler<PatchPiiCommand, PatchPiiResult>
{
    private readonly ProfileDbContext _context;
    private readonly PiiDbContext _piiDbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public PatchPiiHandler(ProfileDbContext context, IPublishEndpoint publishEndpoint, PiiDbContext piiDbContext)
    {
        _context = context;
        _publishEndpoint = publishEndpoint;
        _piiDbContext = piiDbContext;
    }

    public async Task<PatchPiiResult> Handle(PatchPiiCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Pii;

        var piiProfile = await _piiDbContext.PersonProfiles
                      .FirstOrDefaultAsync(a => a.SubjectRef == request.SubjectRef, cancellationToken: cancellationToken)
                  ?? throw new ProfileNotFoundException();
        
        var newContactInfo = ContactInfo.UpdateWithFallback(piiProfile.ContactInfo, dto.ContactInfo?.Address,
            dto.ContactInfo?.Email, dto.ContactInfo?.PhoneNumber);
        
        piiProfile.Update(
            dto.FullName,
            dto.Gender ?? piiProfile.Gender,
            dto.BirthDate ?? piiProfile.BirthDate,
            newContactInfo
            );

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        var piiUpdatedIntegrationEvent = new PiiUpdatedIntegrationEvent(
            piiProfile.SubjectRef,
            piiProfile.FullName?.Value!,
            piiProfile.ContactInfo.Email,
            piiProfile.ContactInfo.PhoneNumber!
        );

        await _publishEndpoint.Publish(piiUpdatedIntegrationEvent, cancellationToken);

        return new PatchPiiResult(result);
    }
}