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

            // Check the same time
            if (_context.DoctorAvailabilities.Any(d => d.DoctorId == dto.DoctorId && d.Date == dto.Date && d.StartTime == dto.StartTime))
                throw new BadRequestException("Doctor is already booked for this time slot.");

            var doctorAvailability = new DoctorAvailability
            {
                Id = Guid.NewGuid(),
                DoctorId = dto.DoctorId,
                Date = dto.Date,
                StartTime = dto.StartTime
            };

            _context.DoctorAvailabilities.Add(doctorAvailability);
            await _context.SaveChangesAsync(cancellationToken);


            return new CreateDoctorAvailabilityResult(doctorAvailability.Id);


        }
    }
}
