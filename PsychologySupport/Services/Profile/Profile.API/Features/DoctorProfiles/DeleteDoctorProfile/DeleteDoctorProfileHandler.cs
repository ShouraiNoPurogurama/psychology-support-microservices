using BuildingBlocks.CQRS;
using Profile.API.Data;
using Profile.API.Exceptions;

namespace Profile.API.Features.DoctorProfiles.DeleteDoctorProfile;

public record DeleteDoctorProfileCommand(Guid Id) : ICommand<DeleteDoctorProfileResult>;

public record DeleteDoctorProfileResult(bool Id);

public class DeleteDoctorProfileHandler : ICommandHandler<DeleteDoctorProfileCommand, DeleteDoctorProfileResult>
{
    private readonly ProfileDbContext _context;

    public DeleteDoctorProfileHandler(ProfileDbContext context)
    {
        _context = context;
    }
    
    public async Task<DeleteDoctorProfileResult> Handle(DeleteDoctorProfileCommand request, CancellationToken cancellationToken)
    {
        var existingDoctorProfile = await _context.DoctorProfiles.FindAsync(request.Id)
                                    ?? throw new ProfileNotFoundException("Doctor Profile", request.Id);

         _context.DoctorProfiles.Remove(existingDoctorProfile);

         var result = await _context.SaveChangesAsync() > 0;
        
        return new DeleteDoctorProfileResult(result);
    }
}