using BuildingBlocks.Data.Common;
using FluentValidation;
using Profile.API.Data.Pii;
using Profile.API.Enums.Pii;

namespace Profile.API.Domains.Pii.Features.OnboardingPersonProfile;

public record OnboardingPersonProfileCommand(
    Guid SubjectRef,
    UserGender Gender,
    DateOnly BirthDate,
    string Address
) : ICommand<OnboardingPersonProfileResult>;

public record OnboardingPersonProfileResult(bool IsSuccess);

public class OnboardingPersonProfileValidator : AbstractValidator<OnboardingPersonProfileCommand>
{
    public OnboardingPersonProfileValidator()
    {
        RuleFor(p => p.SubjectRef).NotEmpty().WithMessage("Reference người dùng không được để trống.");
        RuleFor(p => p.Gender).IsInEnum();
        RuleFor(p => p.BirthDate)
            .LessThan(DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-13)))
            .WithMessage("Người dùng phải từ 13 tuổi trở lên.");
        RuleFor(p => p.Address)
            .NotEmpty()
            .WithMessage("Địa chỉ không được để trống.")
            .MaximumLength(255)
            .WithMessage("Địa chỉ không được vượt quá 255 ký tự.");
    }
}

public class OnboardingPersonProfileHandler(PiiDbContext dbContext, ILogger<OnboardingPersonProfileHandler> logger)
    : ICommandHandler<OnboardingPersonProfileCommand, OnboardingPersonProfileResult>
{
    public async Task<OnboardingPersonProfileResult> Handle(OnboardingPersonProfileCommand request,
        CancellationToken cancellationToken)
    {
        var existingProfile = await dbContext.PersonProfiles
            .FirstOrDefaultAsync(p => p.SubjectRef == request.SubjectRef, cancellationToken: cancellationToken);

        if (existingProfile == null)
        {
            throw new NotFoundException("Không tìm thấy hồ sơ cá nhân để onboarding.", "PERSON_PROFILE_NOT_FOUND");
        }

        if (existingProfile.Status == PersonProfileStatus.Active)
        {
            throw new ConflictException("Hồ sơ cá nhân đã được kích hoạt và không thể onboarding lại.",
                "PERSON_PROFILE_ALREADY_ACTIVE");
        }

        var currentContactInfo = ContactInfo.UpdateWithFallback(existingProfile.ContactInfo, request.Address);

        existingProfile.CompleteOnboarding(
            gender: request.Gender,
            birthDate: request.BirthDate,
            contactInfo: currentContactInfo
        );

        var result = await dbContext.SaveChangesAsync(cancellationToken) > 0;


        return new OnboardingPersonProfileResult(result);
    }
}