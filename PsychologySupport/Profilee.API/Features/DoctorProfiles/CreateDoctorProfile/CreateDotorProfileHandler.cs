using BuildingBlocks.CQRS;
using Profilee.API.Data;
using Profilee.API.Models;

namespace Profilee.API.Features.DoctorProfiles.CreateDoctorProfile
{
    public record CreateDoctorProfileCommand(DoctorProfile DoctorProfile) : ICommand<CreateDoctorProfileResult>;

    public record CreateDoctorProfileResult(Guid Id);
    public class CreateDotorProfileHandler : ICommandHandler<CreateDoctorProfileCommand, CreateDoctorProfileResult>
    {
        private readonly ProfileDbContext dbContext;

        public CreateDotorProfileHandler(ProfileDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<CreateDoctorProfileResult> Handle(CreateDoctorProfileCommand request, CancellationToken cancellationToken)
        {
            dbContext.DoctorProfiles.Add(request.DoctorProfile);

            await dbContext.SaveChangesAsync(cancellationToken);

            return new CreateDoctorProfileResult(request.DoctorProfile.Id);
        }
    }
}
