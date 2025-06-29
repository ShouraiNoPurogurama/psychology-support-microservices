using BuildingBlocks.Data.Common;
using FluentValidation;
using Profile.API.DoctorProfiles.Dtos;

namespace Profile.API.DoctorProfiles.Validators
{
    public class UpdateDoctorProfileValidator : AbstractValidator<UpdateDoctorProfileDto>
    {
        public UpdateDoctorProfileValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Không được để trống trường Họ và tên.")
                .MaximumLength(100).WithMessage("Họ và tên không được vượt quá 100 ký tự.");

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Giới tính không hợp lệ.");

            RuleFor(x => x.ContactInfo)
                .SetValidator(new ContactInfoValidator()!)
                .When(x => x.ContactInfo is not null);

            RuleFor(x => x.SpecialtyIds)
                .Must(list => list == null || list.All(id => id != Guid.Empty))
                .WithMessage("Danh sách chuyên khoa không được chứa GUID rỗng.");

            RuleFor(x => x.Qualifications)
                .MaximumLength(500).WithMessage("Thông tin bằng cấp không được vượt quá 500 ký tự.");

            RuleFor(x => x.YearsOfExperience)
                .GreaterThanOrEqualTo(0).WithMessage("Số năm kinh nghiệm phải là số không âm.")
                .When(x => x.YearsOfExperience.HasValue);

            RuleFor(x => x.Bio)
                .MaximumLength(1000).WithMessage("Tiểu sử không được vượt quá 1000 ký tự.");
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
                .Matches(@"^\+?[0-9]{7,15}$").WithMessage("Số điện thoại không đúng định dạng.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Không được để trống trường Email.")
                .EmailAddress().WithMessage("Email không đúng định dạng.");
        }
    }
}
