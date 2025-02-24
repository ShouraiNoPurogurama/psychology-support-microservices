using BuildingBlocks.CQRS;
using LifeStyles.API.Events;
using LifeStyles.API.Exceptions;
using MassTransit;
using MediatR;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.CreatePatientEntertainmentActivity
{
    public record CreatePatientEntertainmentActivityCommand(Models.PatientEntertainmentActivity PatientEntertainmentActivity)
        : ICommand<CreatePatientEntertainmentActivityResult>;

    public record CreatePatientEntertainmentActivityResult(bool IsSucceeded);

    public class CreatePatientEntertainmentActivityHandler
        : IRequestHandler<CreatePatientEntertainmentActivityCommand, CreatePatientEntertainmentActivityResult>
    {
        private readonly LifeStylesDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRequestClient<CheckPatientProfileExistenceEvent> _client;

        public CreatePatientEntertainmentActivityHandler(
            LifeStylesDbContext context,
            IPublishEndpoint publishEndpoint,
            IRequestClient<CheckPatientProfileExistenceEvent> client)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _client = client;
        }

        public async Task<CreatePatientEntertainmentActivityResult> Handle(
            CreatePatientEntertainmentActivityCommand request,
            CancellationToken cancellationToken)
        {

            // Check patientid
            var patientProfileId = request.PatientEntertainmentActivity.PatientProfileId;

            var response = await _client.GetResponse<CheckPatientProfileExistenceResponseEvent>(
                new CheckPatientProfileExistenceEvent(patientProfileId), cancellationToken);


            if (!response.Message.Exists)
            {
                throw new LifeStylesNotFoundException("PatientProfile", patientProfileId);
            }

            var activity = new Models.PatientEntertainmentActivity
            {
                PatientProfileId = request.PatientEntertainmentActivity.PatientProfileId,
                EntertainmentActivityId = request.PatientEntertainmentActivity.EntertainmentActivityId,
                PreferenceLevel = request.PatientEntertainmentActivity.PreferenceLevel
            };

            _context.PatientEntertainmentActivities.Add(activity);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreatePatientEntertainmentActivityResult(true);
        }
    }
}
