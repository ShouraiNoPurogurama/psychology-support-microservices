using FluentValidation;
using Scheduling.API.Dtos;

namespace Scheduling.API.Validators
{
    public class CreateDoctorAvailabilityValidator : AbstractValidator<CreateDoctorAvailabilityDto>
    {
        public CreateDoctorAvailabilityValidator()
        {
            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage("DoctorId is required.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.")
                .Must(BeAtLeastOneWeekLater).WithMessage("Date must be at least 7 days from today (Vietnam time).");

            RuleFor(x => x.StartTimes)
                .NotNull().WithMessage("StartTimes list is required.")
                .NotEmpty().WithMessage("StartTimes list cannot be empty.")
                .Must(times => times.All(t => t.Hour >= 0 && t.Hour < 24)).WithMessage("StartTimes must be valid 24-hour format.");
        }

        private bool BeAtLeastOneWeekLater(DateOnly date)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // GMT+7
            var vietnamNow = TimeZoneInfo.ConvertTime(DateTime.UtcNow, vietnamTimeZone);
            var oneWeekLater = DateOnly.FromDateTime(vietnamNow).AddDays(7);
            return date >= oneWeekLater;
        }
    }
}
