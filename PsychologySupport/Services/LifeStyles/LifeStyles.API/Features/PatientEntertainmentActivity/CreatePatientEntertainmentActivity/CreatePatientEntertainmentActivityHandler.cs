using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Data.Common;
using MediatR;

namespace LifeStyles.API.Features.PatientEntertainmentActivity.CreatePatientEntertainmentActivity
{
    public record CreatePatientEntertainmentActivityCommand(Guid PatientProfileId, List<(Guid EntertainmentActivityId, PreferenceLevel PreferenceLevel)> Activities)
    : ICommand<CreatePatientEntertainmentActivityResult>;

    public record CreatePatientEntertainmentActivityResult(bool IsSucceeded);

    public class CreatePatientEntertainmentActivityHandler
    : IRequestHandler<CreatePatientEntertainmentActivityCommand, CreatePatientEntertainmentActivityResult>
    {
        private readonly LifeStylesDbContext _context;

        public CreatePatientEntertainmentActivityHandler(LifeStylesDbContext context)
        {
            _context = context;
        }

        public async Task<CreatePatientEntertainmentActivityResult> Handle(
            CreatePatientEntertainmentActivityCommand request,
            CancellationToken cancellationToken)
        {
          

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
