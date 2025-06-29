using BuildingBlocks.Exceptions;
using MediatR;
using Scheduling.API.Dtos;
using Scheduling.API.Models;

namespace Scheduling.API.Features.RegisterDoctorBusyAllDay
{ 
    public record RegisterDoctorBusyAllDayCommand(RegisterDoctorBusyAllDayDto DoctorBusyDto) : IRequest<RegisterDoctorBusyAllDayResult>;

    public record RegisterDoctorBusyAllDayResult(bool Success);

    public class RegisterDoctorBusyAllDayHandler : IRequestHandler<RegisterDoctorBusyAllDayCommand, RegisterDoctorBusyAllDayResult>
    {
        private readonly SchedulingDbContext _context;

        public RegisterDoctorBusyAllDayHandler(SchedulingDbContext context)
        {
            _context = context;
        }

        public async Task<RegisterDoctorBusyAllDayResult> Handle(RegisterDoctorBusyAllDayCommand request, CancellationToken cancellationToken)
        {
            var dto = request.DoctorBusyDto;
            var doctorSlot = await _context.DoctorSlotDurations.FirstOrDefaultAsync(d => d.DoctorId == dto.DoctorId, cancellationToken);
            if (doctorSlot == null)
                throw new NotFoundException($"Chưa thiết lập được thời gian mỗi lượt khám cho bác sĩ (ID: {dto.DoctorId}).");

            var dayOfWeek = dto.Date.DayOfWeek;
            var timeSlotTemplates = await _context.TimeSlotTemplates
                .Where(t => t.DayOfWeek == dayOfWeek)
                .ToListAsync(cancellationToken);

            var doctorAvailabilities = timeSlotTemplates.SelectMany(template =>
                Enumerable.Range(0, (int)(template.EndTime - template.StartTime).TotalMinutes / doctorSlot.SlotDuration)
                    .Select(i => new DoctorAvailability
                    {
                        Id = Guid.NewGuid(),
                        DoctorId = dto.DoctorId,
                        Date = dto.Date,
                        StartTime = template.StartTime.AddMinutes(i * doctorSlot.SlotDuration)
                    })
            ).ToList();

            if (doctorAvailabilities.Any())
            {
                _context.DoctorAvailabilities.AddRange(doctorAvailabilities);
                await _context.SaveChangesAsync(cancellationToken);
                return new RegisterDoctorBusyAllDayResult(true);
            }

            return new RegisterDoctorBusyAllDayResult(false);
        }
    }
}
