using BuildingBlocks.CQRS;
using Mapster;
using Profile.API.Data;
using Profile.API.Dtos;
using Profile.API.Exceptions;
using Profile.API.Models;

namespace Profile.API.Features.DoctorProfiles.UpdateDoctorProfile;

public record UpdateDoctorProfileCommand(DoctorProfileDto DoctorProfile) : ICommand<UpdateDoctorProfileResult>;

public record UpdateDoctorProfileResult(bool IsSuccess);

public class UpdateDoctorProfileHandler : ICommandHandler<UpdateDoctorProfileCommand, UpdateDoctorProfileResult>
{
    private readonly ProfileDbContext _context;

    public UpdateDoctorProfileHandler(ProfileDbContext context)
    {
        _context = context;
    }

    public async Task<UpdateDoctorProfileResult> Handle(UpdateDoctorProfileCommand request, CancellationToken cancellationToken)
    {
        var existingProfile = await _context.DoctorProfiles.FindAsync(request.DoctorProfile.Id)
                              ?? throw new ProfileNotFoundException("Doctor Profile", request.DoctorProfile.Id);

        existingProfile = request.DoctorProfile.Adapt(existingProfile);
        existingProfile.LastModified = DateTime.UtcNow;

        _context.Update(existingProfile);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        return new UpdateDoctorProfileResult(result);
    }
}