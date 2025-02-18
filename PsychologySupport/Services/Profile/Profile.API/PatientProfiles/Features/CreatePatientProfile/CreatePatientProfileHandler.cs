using BuildingBlocks.CQRS;
using Mapster;
using MediatR;
using Profile.API.Data;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Events;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.CreatePatientProfile
{
    public record CreatePatientProfileCommand(PatientProfileCreate PatientProfileDto) : ICommand<CreatePatientProfileResult>;

    public record CreatePatientProfileResult(Guid Id);

    public class CreatePatientProfileHandler : ICommandHandler<CreatePatientProfileCommand, CreatePatientProfileResult>
    {
        private readonly ProfileDbContext _context;
        private readonly IMediator _mediator;

        public CreatePatientProfileHandler(ProfileDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<CreatePatientProfileResult> Handle(CreatePatientProfileCommand request, CancellationToken cancellationToken)
        {
            var patientProfile = request.PatientProfileDto.Adapt<PatientProfile>();

            patientProfile.CreatedAt = DateTimeOffset.UtcNow;


            _context.PatientProfiles.Add(patientProfile);

            await _context.SaveChangesAsync(cancellationToken);

            var patientProfileCreatedEvent = new PatientProfileCreatedEvent(
                patientProfile.UserId,
                patientProfile.Gender,
                patientProfile.ContactInfo.Email,
                patientProfile.ContactInfo.PhoneNumber,
                patientProfile.Allergies,
                patientProfile.CreatedAt
            );

            await _mediator.Publish(patientProfileCreatedEvent, cancellationToken);

            return new CreatePatientProfileResult(patientProfile.Id);
        }

    }
}
