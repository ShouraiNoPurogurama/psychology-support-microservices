using BuildingBlocks.CQRS;
using MediatR;
using Profile.API.DoctorProfiles.Dtos;
using Profile.API.DoctorProfiles.Events;
using Profile.API.Exceptions;


namespace Profile.API.DoctorProfiles.Features.UpdateDoctorProfile;

public record UpdateDoctorProfileCommand(Guid Id, DoctorProfileDto DoctorProfileDto) : ICommand<UpdateDoctorProfileResult>;

public record UpdateDoctorProfileResult(Guid Id);

public class UpdateDoctorProfileHandler : ICommandHandler<UpdateDoctorProfileCommand, UpdateDoctorProfileResult>
{
    private readonly ProfileDbContext _context;
    private readonly IMediator _mediator;

    public UpdateDoctorProfileHandler(ProfileDbContext context, IMediator mediator)
    {
        _context = context;
        _mediator = mediator;
    }

    public async Task<UpdateDoctorProfileResult> Handle(UpdateDoctorProfileCommand request, CancellationToken cancellationToken)
    {
        var doctorProfile = await _context.DoctorProfiles
            .FirstOrDefaultAsync(d => d.Id == request.Id, cancellationToken)
            ?? throw new ProfileNotFoundException("Doctor profile", request.Id);

        doctorProfile.Update(
        request.DoctorProfileDto.FullName,
        request.DoctorProfileDto.Gender,
        request.DoctorProfileDto.ContactInfo,
        request.DoctorProfileDto.Specialty,
        request.DoctorProfileDto.Qualifications,
        request.DoctorProfileDto.YearsOfExperience,
        request.DoctorProfileDto.Bio,
        request.DoctorProfileDto.Rating,
        request.DoctorProfileDto.TotalReviews
    );

        await _context.SaveChangesAsync(cancellationToken);

        var doctorProfileUpdatedEvent = new DoctorProfileUpdatedEvent(
            doctorProfile.Id,
            doctorProfile.Specialty,
            doctorProfile.Qualifications,
            doctorProfile.YearsOfExperience,
            doctorProfile.Bio
        );

        await _mediator.Publish(doctorProfileUpdatedEvent, cancellationToken);

        return new UpdateDoctorProfileResult(doctorProfile.Id);
    }
}
