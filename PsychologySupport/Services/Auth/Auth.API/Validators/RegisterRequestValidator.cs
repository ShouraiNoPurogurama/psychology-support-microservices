using System.Data;
using Auth.API.Dtos.Requests;
using BuildingBlocks.Constants;
using FluentValidation;

namespace Auth.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleForEach(rr => new[] { rr.FullName, rr.Password, rr.ConfirmPassword, rr.PhoneNumber })
            .NotEmpty().WithMessage("{PropertyName} không được để trống.");
        
        RuleFor(rr => rr.Password)
            .Matches(MyPatterns.Password).WithMessage("Mật khẩu phải chứa ít nhất 8 kí tự, bao gồm số, chữ thường và chữ hoa.")
            .Equal(rr => rr.ConfirmPassword).WithMessage("Mật khẩu và Xác nhận mật khẩu không trùng khớp.");
        
        RuleFor(rr => rr.PhoneNumber)
            .Matches(MyPatterns.PhoneNumber).WithMessage("Định dạng số điện thoại không hợp lệ.");

        RuleFor(rr => rr.Email)
            .EmailAddress()
            .WithMessage("Định dạng email không hợp lệ");
    }
}