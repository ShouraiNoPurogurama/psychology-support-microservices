using BuildingBlocks.Data.Common;
using FluentValidation;
using Profile.API.Domains.PatientProfiles.Dtos;

namespace Profile.API.Domains.PatientProfiles.Validators
{
    public class UpdatePatientProfileValidator : AbstractValidator<UpdatePatientProfileDto>
    {
        public UpdatePatientProfileValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Không được để trống trường Họ và tên.")
                .MaximumLength(100).WithMessage("Họ và tên không được vượt quá 100 ký tự.");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Giới tính không hợp lệ.");

            RuleFor(x => x.Allergies)
                .MaximumLength(500).WithMessage("Thông tin dị ứng không được vượt quá 500 ký tự.");

            RuleFor(x => x.PersonalityTraits)
                .IsInEnum().WithMessage("Tính cách không hợp lệ.");

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
                .NotEmpty().WithMessage("Không được để trống trường Địa chỉ.")
                .MaximumLength(200).WithMessage("Địa chỉ không được vượt quá 200 ký tự.");

            RuleFor(x => x.PhoneNumber)
                .NotEmpty().WithMessage("Không được để trống trường Số điện thoại.")
                .Matches(@"^\+?[0-9]{7,15}$").WithMessage("Số điện thoại không đúng định dạng hợp lệ.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Không được để trống trường Email.")
                .EmailAddress().WithMessage("Email không đúng định dạng.");
        }
    }
}