using BuildingBlocks.Constants;
using FluentValidation;

namespace Auth.API.Domains.Authentication.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(rr => rr.Password)
            .Matches(MyPatterns.Password)
            .WithMessage("Mật khẩu phải chứa ít nhất 8 kí tự, bao gồm số, chữ thường và chữ hoa.");

        //Validate PhoneNumber (only if provided)
        RuleFor(rr => rr.PhoneNumber)
            .NotEmpty()
            .When(rr => string.IsNullOrWhiteSpace(rr.Email))
            .WithMessage("Số điện thoại không được để trống nếu email không được cung cấp.")
            .Matches(MyPatterns.PhoneNumber)
            .WithMessage("Định dạng số điện thoại không hợp lệ.");

        //Validate Email (only if provided)
        RuleFor(rr => rr.Email)
            .NotEmpty()
            .When(rr => string.IsNullOrWhiteSpace(rr.PhoneNumber))
            .WithMessage("Email không được để trống nếu số điện thoại không được cung cấp.")
            .EmailAddress()
            .WithMessage("Định dạng email không hợp lệ.");
    }
}