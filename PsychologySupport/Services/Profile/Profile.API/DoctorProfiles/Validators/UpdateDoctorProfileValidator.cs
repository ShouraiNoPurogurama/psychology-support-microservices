using FluentValidation;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Validators
{
    public class UpdateDoctorProfileValidator : AbstractValidator<UpdateDoctorProfileDto>
    {
        public UpdateDoctorProfileValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName is required.")
                .MaximumLength(100).WithMessage("FullName must not exceed 100 characters.");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender value.");

            RuleFor(x => x.ContactInfo)
                .SetValidator(new ContactInfoValidator()!)
                .When(x => x.ContactInfo is not null);

            RuleFor(x => x.SpecialtyIds)
                .Must(list => list == null || list.All(id => id != Guid.Empty))
                .WithMessage("SpecialtyIds cannot contain empty GUIDs.");

            RuleFor(x => x.Qualifications)
                .MaximumLength(500).WithMessage("Qualifications must not exceed 500 characters.");

            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage("Years of experience must be a positive number.")
                .When(x => x.YearsOfExperience.HasValue);

            RuleFor(x => x.Bio)
                .MaximumLength(1000).WithMessage("Bio must not exceed 1000 characters.");
        }
    }

    public class ContactInfoValidator : AbstractValidator<ContactInfo>
    {
        public ContactInfoValidator()
        {
            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MaximumLength(200).WithMessage("Address must not exceed 200 characters.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("PhoneNumber is required.")
                .Matches(@"^\+?[0-9]{7,15}$").WithMessage("PhoneNumber must be a valid format.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Email must be a valid email address.");
        }
    }
}
