using BuildingBlocks.CQRS;
using Profile.API.Data;
using Profile.API.Exceptions;

namespace Profile.API.Features.PatientProfiles.DeletePatientProfile
{
    public record DeletePatientProfileCommand(Guid Id) : ICommand<DeletePatientProfileResult>;

    public record DeletePatientProfileResult(bool IsSuccess);
    public class DeletePatientProfileHandler : ICommandHandler<DeletePatientProfileCommand, DeletePatientProfileResult>
    {
        private readonly ProfileDbContext _context;

        public DeletePatientProfileHandler(ProfileDbContext context)
        {
            _context = context;
        }
        public async Task<DeletePatientProfileResult> Handle(DeletePatientProfileCommand request, CancellationToken cancellationToken)
        {
            var existingPatientProfile = await _context.PatientProfiles.FindAsync(request.Id)
                                   ?? throw new ProfileNotFoundException("Patient Profile", request.Id);

            _context.PatientProfiles.Remove(existingPatientProfile);

            var result = await _context.SaveChangesAsync() > 0;

            return new DeletePatientProfileResult(result);
        }
    }
}
