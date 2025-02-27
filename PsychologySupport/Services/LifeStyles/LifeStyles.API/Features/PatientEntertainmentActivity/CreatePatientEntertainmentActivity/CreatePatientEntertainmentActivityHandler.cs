using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Data.Common;
using LifeStyles.API.Events;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.CreatePatientEntertainmentActivity
{
    public record CreatePatientEntertainmentActivityCommand(Guid PatientProfileId, List<(Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel)> Activities)
    : ICommand<CreatePatientEntertainmentActivityResult>;

    public record CreatePatientEntertainmentActivityResult(bool IsSucceeded);

    public class CreatePatientEntertainmentActivityHandler
    : ICommandHandler<CreatePatientEntertainmentActivityCommand, CreatePatientEntertainmentActivityResult>
    {
        private readonly LifeStylesDbContext _context;
        private readonly IRequestClient<CheckPatientProfileExistenceIntegrationEvent> _client;

        public CreatePatientEntertainmentActivityHandler(LifeStylesDbContext context,
            IRequestClient<CheckPatientProfileExistenceIntegrationEvent> client)
        {
            _context = context;
            _client = client;
        }

        public async Task<CreatePatientEntertainmentActivityResult> Handle(
            CreatePatientEntertainmentActivityCommand request,CancellationToken cancellationToken)
        {

            var response = await _client.GetResponse<CheckPatientProfileExistenceResponseEvent>(new CheckPatientProfileExistenceIntegrationEvent(request.PatientProfileId), cancellationToken);

            if (!response.Message.Exists)
            {
                throw new LifeStylesNotFoundException("PatientProfile", request.PatientProfileId);
            }

            var activities = request.Activities
                .Select(activity => new Models.PatientEntertainmentActivity
                {
                    PatientProfileId = request.PatientProfileId,
                    EntertainmentActivityId = activity.EntertainmentActivityId,
                    PreferenceLevel = activity.PreferenceLevel
                }).ToList();

            _context.PatientEntertainmentActivities.AddRange(activities);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreatePatientEntertainmentActivityResult(true);
        }
    }

}
