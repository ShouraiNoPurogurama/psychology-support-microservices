using BuildingBlocks.CQRS;
using BuildingBlocks.Messaging.Events.Profile;
using LifeStyles.API.Data;
using LifeStyles.API.Data.Common;
using LifeStyles.API.Events;
using LifeStyles.API.Exceptions;
using MassTransit;

namespace LifeStyles.API.Features.PatientPhysicalActivity.CreatePatientPhysicalActivity
{
    public record CreatePatientPhysicalActivityCommand(
        Guid PatientProfileId,
        List<(Guid PhysicalActivityId, PreferenceLevel PreferenceLevel)> Activities
    ) : ICommand<CreatePatientPhysicalActivityResult>;

    public record CreatePatientPhysicalActivityResult(bool IsSucceeded);

    public class CreatePatientPhysicalActivityHandler : ICommandHandler<CreatePatientPhysicalActivityCommand,
        CreatePatientPhysicalActivityResult>
    {
        private readonly LifeStylesDbContext _context;
        private readonly IRequestClient<CheckPatientProfileExistenceIntegrationEvent> _client;

        public CreatePatientPhysicalActivityHandler(LifeStylesDbContext context,
            IRequestClient<CheckPatientProfileExistenceIntegrationEvent> client)
        {
            _context = context;
            _client = client;
        }

        public async Task<CreatePatientPhysicalActivityResult> Handle(CreatePatientPhysicalActivityCommand request,
            CancellationToken cancellationToken)
        {
            var response = await _client.GetResponse<CheckPatientProfileExistenceResponseEvent>(new CheckPatientProfileExistenceIntegrationEvent(request.PatientProfileId), cancellationToken);

            if (!response.Message.Exists)
            {
                throw new LifeStylesNotFoundException("PatientProfile", request.PatientProfileId);
            }


            var activities = request.Activities.Select(a => new Models.PatientPhysicalActivity
                {
                    PatientProfileId = request.PatientProfileId,
                    PhysicalActivityId = a.PhysicalActivityId,
                    PreferenceLevel = a.PreferenceLevel
                })
                .ToList();

            _context.PatientPhysicalActivities.AddRange(activities);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreatePatientPhysicalActivityResult(true);
        }
    }
}