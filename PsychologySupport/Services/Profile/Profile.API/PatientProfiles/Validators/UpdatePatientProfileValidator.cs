using BuildingBlocks.Data.Common;
using FluentValidation;
using Profile.API.PatientProfiles.Dtos;

namespace Profile.API.PatientProfiles.Validators
{
    public class UpdatePatientProfileValidator : AbstractValidator<UpdatePatientProfileDto>
    {
        public UpdatePatientProfileValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("FullName is required.")
                .MaximumLength(100).WithMessage("FullName must not exceed 100 characters.");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender value.");

            RuleFor(x => x.Allergies)
                .MaximumLength(500).WithMessage("Allergies must not exceed 500 characters.");

            RuleFor(x => x.PersonalityTraits)
                .IsInEnum().WithMessage("Invalid personality trait value.");

            RuleFor(x => x.ContactInfo)
                .SetValidator(new ContactInfoValidator()!)
                .When(x => x.ContactInfo is not null);
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
