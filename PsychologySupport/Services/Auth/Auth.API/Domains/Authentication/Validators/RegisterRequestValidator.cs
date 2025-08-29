using Auth.API.Domains.Authentication.Dtos.Requests;
using BuildingBlocks.Constants;
using FluentValidation;

namespace Auth.API.Domains.Authentication.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(rr => rr.FullName)
            .NotEmpty().WithMessage("Họ và tên không được để trống.");

        RuleFor(rr => rr.Password)
            .NotEmpty().WithMessage("Mật khẩu không được để trống.")
            .Matches(MyPatterns.Password).WithMessage("Mật khẩu phải chứa ít nhất 8 kí tự, bao gồm số, chữ thường và chữ hoa.");

        RuleFor(rr => rr.ConfirmPassword)
            .NotEmpty().WithMessage("Xác nhận mật khẩu không được để trống.");

        RuleFor(rr => rr.Password)
            .Equal(rr => rr.ConfirmPassword)
            .WithMessage("Mật khẩu và Xác nhận mật khẩu không trùng khớp.");

        RuleFor(rr => rr.PhoneNumber)
            .NotEmpty().WithMessage("Số điện thoại không được để trống.")
            .Matches(MyPatterns.PhoneNumber).WithMessage("Định dạng số điện thoại không hợp lệ.");

        RuleFor(rr => rr.Email)
            .NotEmpty().WithMessage("Email không được để trống.")
            .EmailAddress().WithMessage("Định dạng email không hợp lệ");
    }
}
