using BuildingBlocks.Exceptions;
using MediatR;
using Scheduling.API.Dtos;
using Scheduling.API.Models;

namespace Scheduling.API.Features.CreateDoctorAvailability
{
    public record CreateDoctorAvailabilityCommand(CreateDoctorAvailabilityDto DoctorAvailabilityCreate) : IRequest<CreateDoctorAvailabilityResult>;

    public record CreateDoctorAvailabilityResult(Guid Id);

    public class CreateDoctorAvailabilityHandler : IRequestHandler<CreateDoctorAvailabilityCommand, CreateDoctorAvailabilityResult>
    {
        private readonly SchedulingDbContext _context;

        public CreateDoctorAvailabilityHandler(SchedulingDbContext context)
        {
            _context = context;
        }

        public async Task<CreateDoctorAvailabilityResult> Handle(CreateDoctorAvailabilityCommand request, CancellationToken cancellationToken)
        {
            var dto = request.DoctorAvailabilityCreate;

            var existingAvailabilities = _context.DoctorAvailabilities
                .Where(d => d.DoctorId == dto.DoctorId && d.Date == dto.Date && dto.StartTimes.Contains(d.StartTime))
                .Select(d => d.StartTime)
                .ToList();

            if (existingAvailabilities.Any())
                throw new BadRequestException($"Bác sĩ đã có lịch vào các khung giờ sau: {string.Join(", ", existingAvailabilities)}. Vui lòng chọn khung giờ khác.");

            var doctorAvailabilities = dto.StartTimes.Select(startTime => new DoctorAvailability
            {
                Id = Guid.NewGuid(),
                DoctorId = dto.DoctorId,
                Date = dto.Date,
                StartTime = startTime
            }).ToList();

            _context.DoctorAvailabilities.AddRange(doctorAvailabilities);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateDoctorAvailabilityResult(doctorAvailabilities.First().Id);
        }
    }
}

