using BuildingBlocks.CQRS;
using Profile.API.Data;
using Profile.API.Models;

namespace Profile.API.Features.DoctorProfiles.CreateDoctorProfile;

public record CreateDoctorProfileCommand(DoctorProfile DoctorProfile) : ICommand<CreateDoctorProfileResult>;

public record CreateDoctorProfileResult(Guid Id);

public class CreateDoctorProfileHandler : ICommandHandler<CreateDoctorProfileCommand, CreateDoctorProfileResult>
{
    private readonly ProfileDbContext _context;

    public CreateDoctorProfileHandler(ProfileDbContext context)
    {
        _context = context;
    }
    
    public async Task<CreateDoctorProfileResult> Handle(CreateDoctorProfileCommand request, CancellationToken cancellationToken)
    {
        _context.DoctorProfiles.Add(request.DoctorProfile);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateDoctorProfileResult(request.DoctorProfile.Id);
    }
}