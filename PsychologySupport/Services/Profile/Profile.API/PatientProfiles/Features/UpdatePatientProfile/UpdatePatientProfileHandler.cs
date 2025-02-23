using BuildingBlocks.CQRS;
using Mapster;
using MassTransit;
using MediatR;
using Profile.API.Data;
using Profile.API.Exceptions;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Events;

namespace Profile.API.PatientProfiles.Features.UpdatePatientProfile
{
    public record UpdatePatientProfileCommand(Guid Id, UpdatePatientProfileDto PatientProfileUpdate) : ICommand<UpdatePatientProfileResult>;

    public record UpdatePatientProfileResult(Guid Id);

    public class UpdatePatientProfileHandler : ICommandHandler<UpdatePatientProfileCommand, UpdatePatientProfileResult>
    {
        private readonly ProfileDbContext _context;
        private readonly IMediator _mediator;
        private readonly IPublishEndpoint _publishEndpoint;

        public UpdatePatientProfileHandler(ProfileDbContext context, IMediator mediator,IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _mediator = mediator;
            _publishEndpoint = publishEndpoint;
        }

        public async Task<UpdatePatientProfileResult> Handle(UpdatePatientProfileCommand request, CancellationToken cancellationToken)
        {
            var patientProfile = await _context.PatientProfiles.FindAsync(new object[] { request.Id }, cancellationToken);
            if (patientProfile is null)
            {
                throw new KeyNotFoundException("Patient profile not found.");
            }

            var dto = request.PatientProfileUpdate;
            patientProfile.Update(dto.FullName, dto.Gender, dto.Allergies, dto.PersonalityTraits, dto.ContactInfo);
            patientProfile.LastModified = DateTimeOffset.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            var patientProfileUpdatedEvent = new PatientProfileUpdatedEvent(
                patientProfile.UserId,
                patientProfile.FullName,
                patientProfile.Gender.ToString(),
                patientProfile.ContactInfo.Email,
                patientProfile.ContactInfo.PhoneNumber,
                patientProfile.LastModified
            );

            await _mediator.Publish(patientProfileUpdatedEvent, cancellationToken);
            await _publishEndpoint.Publish(patientProfileUpdatedEvent, cancellationToken);

            return new UpdatePatientProfileResult(patientProfile.Id);
        }
    }
}
