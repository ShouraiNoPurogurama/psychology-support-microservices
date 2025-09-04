using BuildingBlocks.Messaging.Events.Queries.LifeStyle;
using LifeStyles.API.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace LifeStyles.API.EventHandler
{
    public class CheckPatientEmotionTodayRequestHandler : IConsumer<CheckPatientEmotionTodayRequest>
    {
        private readonly LifeStylesDbContext _context;

        public CheckPatientEmotionTodayRequestHandler(LifeStylesDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<CheckPatientEmotionTodayRequest> context)
        {
            var request = context.Message;

            var todayVN = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).Date;

            var hasLoggedToday = await _context.PatientEmotionCheckpoints
            .AnyAsync(x => x.PatientProfileId == request.PatientProfileId && x.LogDate.Date == todayVN);

            await context.RespondAsync(new CheckPatientEmotionTodayResponse(hasLoggedToday));
        }
    }
}
