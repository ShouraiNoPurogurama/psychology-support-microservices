using BuildingBlocks.CQRS;
using Mapster;
using MediatR;
using Profile.API.Common.ValueObjects;
using Profile.API.Data;
using Profile.API.DoctorProfiles.Dtos;
using Profile.API.DoctorProfiles.Events;
using Profile.API.DoctorProfiles.Models;


namespace Profile.API.DoctorProfiles.Features.CreateDoctorProfile
{
    public record CreateDoctorProfileCommand(DoctorProfileCreate DoctorProfile) : ICommand<CreateDoctorProfileResult>;

    public record CreateDoctorProfileResult(Guid Id);

    public class CreateDoctorProfileHandler : ICommandHandler<CreateDoctorProfileCommand, CreateDoctorProfileResult>
    {
        private readonly ProfileDbContext _context;
        private readonly IMediator _mediator;

        public CreateDoctorProfileHandler(ProfileDbContext context, IMediator mediator)
        {
            _context = context;
            _mediator = mediator;
        }

        public async Task<CreateDoctorProfileResult> Handle(CreateDoctorProfileCommand request, CancellationToken cancellationToken)
        {
            var doctorProfileCreate = request.DoctorProfile;

            var doctorProfile = DoctorProfile.Create(
                doctorProfileCreate.UserId,
                doctorProfileCreate.FullName,
                doctorProfileCreate.Gender,
                new ContactInfo(
                    doctorProfileCreate.ContactInfo.Email,
                    doctorProfileCreate.ContactInfo.PhoneNumber,
                    doctorProfileCreate.ContactInfo.Address
                ),
                doctorProfileCreate.Specialty,
                doctorProfileCreate.Qualifications,
                doctorProfileCreate.YearsOfExperience,
                doctorProfileCreate.Bio
            );

            doctorProfile.CreatedAt = DateTimeOffset.UtcNow;

            _context.DoctorProfiles.Add(doctorProfile);
            await _context.SaveChangesAsync(cancellationToken);

            var doctorProfileCreatedEvent = new DoctorProfileCreatedEvent(
                doctorProfile.UserId,
                doctorProfile.Gender,
                doctorProfile.ContactInfo.Email,
                doctorProfile.ContactInfo.PhoneNumber,
                doctorProfile.Specialty,
                doctorProfile.CreatedAt
            );

            await _mediator.Publish(doctorProfileCreatedEvent, cancellationToken);

            return new CreateDoctorProfileResult(doctorProfile.Id);
        }

    }
}
