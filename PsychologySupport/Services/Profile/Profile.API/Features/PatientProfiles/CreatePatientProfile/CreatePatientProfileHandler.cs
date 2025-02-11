using BuildingBlocks.CQRS;
using Profile.API.Data;
using Profile.API.Models;

namespace Profile.API.Features.PatientProfiles.CreatePatientProfile
{
    public record CreatePatientProfileCommand(PatientProfile PatientProfile) : ICommand<CreatePatientProfileResult>;
    
    public record CreatePatientProfileResult(Guid Id);
    public class CreatePatientProfileHandler : ICommandHandler<CreatePatientProfileCommand, CreatePatientProfileResult>
    {
        private readonly ProfileDbContext _context;

        public CreatePatientProfileHandler(ProfileDbContext context)
        {
            _context = context;
        }
        public async Task<CreatePatientProfileResult> Handle(CreatePatientProfileCommand request, CancellationToken cancellationToken)
        {
            _context.PatientProfiles.Add(request.PatientProfile);

            await _context.SaveChangesAsync(cancellationToken);

            return new CreatePatientProfileResult(request.PatientProfile.Id);
        }
    }
}
