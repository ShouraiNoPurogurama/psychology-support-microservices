using Profile.API.Common.ValueObjects;
using Profile.API.DoctorProfiles.Dtos;
using Profile.API.Exceptions;


namespace Profile.API.DoctorProfiles.Features.UpdateDoctorProfile;

public record UpdateDoctorProfileCommand(Guid Id, DoctorProfileDto DoctorProfileDto) : ICommand<UpdateDoctorProfileResult>;

public record UpdateDoctorProfileResult(Guid Id);

public class UpdateDoctorProfileHandler : ICommandHandler<UpdateDoctorProfileCommand, UpdateDoctorProfileResult>
{
    private readonly ProfileDbContext _context;
    private readonly IMediator _mediator;
    private readonly IPublishEndpoint _publishEndpoint;

    public UpdateDoctorProfileHandler(ProfileDbContext context, IMediator mediator,IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mediator = mediator;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<UpdateDoctorProfileResult> Handle(UpdateDoctorProfileCommand request, CancellationToken cancellationToken)
    {
        var doctorProfile = await _context.DoctorProfiles.Include(doctorProfile => doctorProfile.ContactInfo)
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
        
        doctorProfile.LastModified = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var doctorProfileUpdatedEvent = new DoctorProfileUpdatedIntegrationEvent(
            doctorProfile.UserId,
            doctorProfile.FullName,
            doctorProfile.Gender,
            doctorProfile.ContactInfo.Email,
            doctorProfile.ContactInfo.PhoneNumber,
            doctorProfile.LastModified
        );

        await _mediator.Publish(doctorProfileUpdatedEvent, cancellationToken);
        await _publishEndpoint.Publish(doctorProfileUpdatedEvent, cancellationToken);

        return new UpdateDoctorProfileResult(doctorProfile.Id);
    }
}
