using FluentValidation;
using Profile.API.Domains.Public.PatientProfiles.Exceptions;

namespace Profile.API.Domains.Public.PatientProfiles.Features.OnboardingPatientProfile;

public record OnboardingPatientProfileCommand(
    Guid PatientId,
    Guid JobId
) : ICommand<OnboardingProfileResult>;

public record OnboardingProfileResult(bool IsSuccess);

public class OnBoardingProfileCommandValidator : AbstractValidator<OnboardingPatientProfileCommand>
{
    public OnBoardingProfileCommandValidator()
    {
        RuleFor(p => p.PatientId).NotEmpty().WithMessage("ID người dùng không được để trống.");
        RuleFor(p => p.JobId).NotEmpty().WithMessage("Công việc không được để trống.");
    }
}

public class OnboardingPatientProfileHandler(ProfileDbContext dbContext, ILogger<OnboardingPatientProfileHandler> logger)
    : ICommandHandler<OnboardingPatientProfileCommand, OnboardingProfileResult>
{
    public async Task<OnboardingProfileResult> Handle(OnboardingPatientProfileCommand request,
        CancellationToken cancellationToken)
    {
        var existingPatient = await dbContext.PatientProfiles
            .FirstOrDefaultAsync(p => p.Id == request.PatientId, cancellationToken: cancellationToken);

        if (existingPatient is null)
        {
            logger.LogError("Không tìm thấy hồ sơ người dùng với ID: {PatientId}", request.PatientId);
            throw new ProfileNotFoundException();
        }

        if (existingPatient.IsProfileCompleted)
        {
            logger.LogInformation("Hồ sơ người dùng với ID: {PatientId} đã được hoàn thiện trước đó. Không thể onboarding lại.",
                request.PatientId);
            throw new ConflictException("Hồ sơ đã được hoàn thiện trước đó. Không thể onboarding lại.",
                "PATIENT_PROFILE_ALREADY_ACTIVE");
        }

        var jobExists = await dbContext.Jobs
            .AnyAsync(j => j.Id == request.JobId, cancellationToken: cancellationToken);

        if (!jobExists)
        {
            logger.LogError("Không tìm thấy công việc với ID: {JobId}", request.JobId);
            throw new BadRequestException("Công việc không hợp lệ.", "INVALID_JOB_ID");
        }

        existingPatient.UpdateJob(request.JobId);

        var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;

        return new OnboardingProfileResult(result);
    }
}