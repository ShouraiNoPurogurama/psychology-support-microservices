using Auth.API.Dtos.Requests;
using BuildingBlocks.Constants;
using FluentValidation;

namespace Auth.API.Validators
{
    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        public ChangePasswordRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống.")
                .EmailAddress().WithMessage("Định dạng email không hợp lệ.");

            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Mật khẩu hiện tại không được để trống.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu mới không được để trống.")
                .Matches(MyPatterns.Password)
                .WithMessage("Mật khẩu mới phải chứa ít nhất 8 kí tự, bao gồm số, chữ thường và chữ hoa.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Xác nhận mật khẩu không được để trống.")
                .Equal(x => x.NewPassword)
                .WithMessage("Xác nhận mật khẩu không khớp với mật khẩu mới.");
        }
    }
}
