using FluentValidation;
using Scheduling.API.Dtos;

namespace Scheduling.API.Validators
{
    public class RegisterDoctorBusyAllDayValidator : AbstractValidator<RegisterDoctorBusyAllDayDto>
    {
        public RegisterDoctorBusyAllDayValidator()
        {
            RuleFor(x => x.DoctorId)
                .NotEmpty().WithMessage("DoctorId is required.");

            RuleFor(x => x.Date)
                .NotEmpty().WithMessage("Date is required.")
                .Must(BeAtLeastOneWeekLater).WithMessage("Date must be at least 7 days from today (Vietnam time).");


        }

        private bool BeAtLeastOneWeekLater(DateOnly date)
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // GMT+7
            var vietnamNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, vietnamTimeZone);
            var oneWeekLater = DateOnly.FromDateTime(vietnamNow.DateTime).AddDays(7);
            return date >= oneWeekLater;
        }
    }
}
