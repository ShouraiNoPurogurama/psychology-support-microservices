using FluentValidation;
using Scheduling.API.Dtos;

namespace Scheduling.API.Validators
{
    public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
    {
        public CreateBookingValidator()
        {
            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage("DoctorId is required.");

            RuleFor(x => x.PatientId)
                .NotEmpty().WithMessage("PatientId is required.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.")
                .Must(BeWithinOneWeek).WithMessage("Date must be within one week from today (Vietnam time).");

            RuleFor(x => x.StartTime)
                .NotEmpty().WithMessage("StartTime is required.");

            RuleFor(x => x.Duration)
                .GreaterThan(0).WithMessage("Duration must be greater than 0.");

        }

        private bool BeWithinOneWeek(DateOnly date)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone).Date;

            return date >= DateOnly.FromDateTime(vietnamNow) &&
                   date <= DateOnly.FromDateTime(vietnamNow.AddDays(7));
        }
    }
}
