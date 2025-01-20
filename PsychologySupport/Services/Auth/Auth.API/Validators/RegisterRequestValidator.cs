using System.Data;
using Auth.API.Dtos.Requests;
using FluentValidation;

namespace Auth.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>
    /// Password must be at least 8 characters long and contain at least one digit, one lowercase letter, one uppercase letter.
    /// </summary>
    private readonly string _passwordPattern = "(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,}$";

    /// <summary>
    /// Phone number must has at 10-12 digits
    /// </summary>
    private readonly string _phonePattern = @"^\+?[0-9]{10,12}$";
    
    public RegisterRequestValidator()
    {
        RuleForEach(rr => new[] { rr.Name, rr.Password, rr.ConfirmPassword, rr.PhoneNumber })
            .NotEmpty().WithMessage("{PropertyName} không được để trống.");
        
        RuleFor(rr => rr.Password)
            .Matches(_passwordPattern).WithMessage("Mật khẩu phải chứa ít nhất 8 kí tự, bao gồm số, chữ thường và chữ hoa.")
            .Equal(rr => rr.ConfirmPassword).WithMessage("Mật khẩu và Xác nhận mật khẩu không trùng khớp.");
        
        RuleFor(rr => rr.PhoneNumber)
            .Matches(_phonePattern).WithMessage("Định dạng số điện thoại không hợp lệ.");

        RuleFor(rr => rr.Email)
            .EmailAddress()
            .WithMessage("Định dạng email không hợp lệ");
    }
}