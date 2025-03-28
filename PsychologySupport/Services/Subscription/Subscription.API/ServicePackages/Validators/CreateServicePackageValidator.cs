using FluentValidation;
// using Profile.API.DoctorProfiles.Dtos;
using Subscription.API.ServicePackages.Dtos;

namespace Subscription.API.ServicePackages.Validators
{
    // public class CreateServicePackageValidator : AbstractValidator<CreateServicePackageDto>
    // {
    //     public CreateServicePackageValidator()
    //     {
    //         RuleFor(x => x.Name)
    //             .NotEmpty().WithMessage("Service package name is required.")
    //             .MaximumLength(100).WithMessage("Service package name must not exceed 100 characters.");
    //
    //         RuleFor(x => x.Description)
    //             .NotEmpty().WithMessage("Description is required.")
    //             .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");
    //
    //         RuleFor(x => x.Price)
    //             .GreaterThan(0).WithMessage("Price must be greater than zero.");
    //
    //         RuleFor(x => x.DurationDays)
    //             .GreaterThan(0).WithMessage("Duration must be greater than zero days.");
    //     }
    // }
}
