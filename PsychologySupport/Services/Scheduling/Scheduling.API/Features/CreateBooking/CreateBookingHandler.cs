namespace Scheduling.API.Features.CreateBooking
{
    using BuildingBlocks.CQRS;
    using BuildingBlocks.Messaging.Events.Profile;
    using MassTransit;
    using Scheduling.API.Data.Common;
    using Scheduling.API.Exceptions;
    using Scheduling.API.Models;

    public record CreateBookingCommand(
        Guid DoctorId,
        Guid PatientId,
        DateOnly Date,
        TimeOnly StartTime,
        int Duration,
        decimal Price,
        Guid? PromoCodeId,
        Guid? GiftCodeId
    ) : ICommand<CreateBookingResult>;

    public record CreateBookingResult(Guid BookingId, string BookingCode);

    public class CreateBookingHandler : ICommandHandler<CreateBookingCommand, CreateBookingResult>
    {
        private readonly IRequestClient<PatientProfileExistenceRequest> _patientClient;
        private readonly IRequestClient<DoctorProfileExistenceRequest> _doctorClient;
        private readonly SchedulingDbContext _context;

        public CreateBookingHandler(
            SchedulingDbContext context,
            IRequestClient<PatientProfileExistenceRequest> patientClient,
            IRequestClient<DoctorProfileExistenceRequest> doctorClient)
        {
            _context = context;
            _patientClient = patientClient;
            _doctorClient = doctorClient;
        }

        public async Task<CreateBookingResult> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
        {
            // check patient
            var patientResponse = await _patientClient.GetResponse<PatientProfileExistenceResponse>(
                new PatientProfileExistenceRequest(request.PatientId), cancellationToken);

            if (!patientResponse.Message.IsExist)
                throw new SchedulingNotFoundException("Patient", request.PatientId);

            // check doctor
            var doctorResponse = await _doctorClient.GetResponse<DoctorProfileExistenceResponse>(
                new DoctorProfileExistenceRequest(request.DoctorId), cancellationToken);

            if (!doctorResponse.Message.IsExist)
                throw new SchedulingNotFoundException("Doctor", request.DoctorId);

            
            var bookingCode = GenerateBookingCode(request.Date);

            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                BookingCode = bookingCode,
                DoctorId = request.DoctorId,
                PatientId = request.PatientId,
                Date = request.Date,
                StartTime = request.StartTime,
                Duration = request.Duration,
                Price = request.Price,
                PromoCodeId = request.PromoCodeId,
                GiftCodeId = request.GiftCodeId,
                Status = BookingStatus.Pending
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreateBookingResult(booking.Id, bookingCode);
        }

        private string GenerateBookingCode(DateOnly date)
        {
            var randomString = GenerateRandomString();
            return $"EE-{randomString}-{date:yyyyMMdd}";
        }

        private static string GenerateRandomString(int length = 6)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            char[] stringChars = new char[length];

            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new string(stringChars);
        }
    }

}
