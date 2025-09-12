using Profile.API.Domains.Public.PatientProfiles.Exceptions;

namespace Profile.API.Domains.Public.PatientProfiles.Features.UpdatePatientJob;

public record UpdatePatientJobCommand(Guid PatientId, Guid JobId) : ICommand<UpdatePatientProfileResult>;

public record UpdatePatientProfileResult(Guid PatientId);

public class UpdatePatientJobHandler(ProfileDbContext dbContext)
    : ICommandHandler<UpdatePatientJobCommand, UpdatePatientProfileResult>
{
    public async Task<UpdatePatientProfileResult> Handle(UpdatePatientJobCommand request, CancellationToken cancellationToken)
    {
        var patientProfile = await dbContext.PatientProfiles
                                 .FirstOrDefaultAsync(p => p.Id == request.PatientId,
                                     cancellationToken: cancellationToken)
                             ?? throw new ProfileNotFoundException();

        bool isValidJob = await dbContext.Jobs
            .AnyAsync(j => j.Id == request.JobId, cancellationToken: cancellationToken);

        if(!isValidJob)
        {
            throw new BadRequestException("Công việc mới không hợp lệ.", "JOB_NOT_FOUND");
        }

        if (request.JobId == patientProfile.JobId)
        {
            
        }
        
        patientProfile.UpdateJob(request.JobId);

        await dbContext.SaveChangesAsync(cancellationToken);

        return new UpdatePatientProfileResult(patientProfile.Id);
    }
}