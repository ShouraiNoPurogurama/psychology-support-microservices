using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Data.Common;
using LifeStyles.API.Events;
using LifeStyles.API.Exceptions;
using MassTransit;
using MediatR;

namespace LifeStyles.API.Features.PatientPhysicalActivity.CreatePatientPhysicalActivity
{
    public record CreatePatientPhysicalActivityCommand(
    Guid PatientProfileId,
    List<(Guid PhysicalActivityId, PreferenceLevel PreferenceLevel)> Activities
) : ICommand<CreatePatientPhysicalActivityResult>;

    public record CreatePatientPhysicalActivityResult(bool IsSucceeded);

    public class CreatePatientPhysicalActivityHandler
      : IRequestHandler<CreatePatientPhysicalActivityCommand, CreatePatientPhysicalActivityResult>
    {
        private readonly LifeStylesDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IRequestClient<CheckPatientProfileExistenceEvent> _client;

        public CreatePatientPhysicalActivityHandler(
            LifeStylesDbContext context,
            IPublishEndpoint publishEndpoint,
            IRequestClient<CheckPatientProfileExistenceEvent> client)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _client = client;
        }

        public async Task<CreatePatientPhysicalActivityResult> Handle(
            CreatePatientPhysicalActivityCommand request,
            CancellationToken cancellationToken)
        {
         
            var response = await _client.GetResponse<CheckPatientProfileExistenceResponseEvent>(
                new CheckPatientProfileExistenceEvent(request.PatientProfileId), cancellationToken);

            if (!response.Message.Exists)
            {
                throw new LifeStylesNotFoundException("PatientProfile", request.PatientProfileId);
            }

           
            var activities = request.Activities.Select(a => new Models.PatientPhysicalActivity
            {
                PatientProfileId = request.PatientProfileId,
                PhysicalActivityId = a.PhysicalActivityId,
                PreferenceLevel = a.PreferenceLevel
            }).ToList();

            _context.PatientPhysicalActivities.AddRange(activities);
            await _context.SaveChangesAsync(cancellationToken);

            return new CreatePatientPhysicalActivityResult(true);
        }
    }

}
