using BuildingBlocks.CQRS;
using Mapster;
using MediatR;
using Profile.API.Data;
using Profile.API.PatientProfiles.Dtos;
using Profile.API.PatientProfiles.Events;
using Profile.API.PatientProfiles.Models;

namespace Profile.API.PatientProfiles.Features.CreatePatientProfile
{
    public record CreatePatientProfileCommand(PatientProfileCreate PatientProfileCreate) : ICommand<CreatePatientProfileResult>;

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
            try
            {
                var dto = request.PatientProfileCreate;

                var patientProfile = PatientProfile.Create(
                    dto.UserId,
                    dto.FullName,
                    dto.Gender,
                    dto.Allergies,
                    dto.PersonalityTraits,
                    dto.ContactInfo
                );
                patientProfile.CreatedAt = DateTimeOffset.UtcNow;

                _context.PatientProfiles.Add(patientProfile);
                await _context.SaveChangesAsync(cancellationToken);

                var patientProfileCreatedEvent = new PatientProfileCreatedEvent(
                    patientProfile.UserId,
                    patientProfile.FullName,
                    patientProfile.Gender,
                    patientProfile.ContactInfo.Email,
                    patientProfile.ContactInfo.PhoneNumber,
                    patientProfile.Allergies,
                    patientProfile.CreatedAt
                );

                await _mediator.Publish(patientProfileCreatedEvent, cancellationToken);

                return new CreatePatientProfileResult(patientProfile.Id);
            }
            catch (DbUpdateException ex)
            {
                throw new Exception($"Database error: {ex.InnerException?.Message}", ex);
            }
        }

    }
}
