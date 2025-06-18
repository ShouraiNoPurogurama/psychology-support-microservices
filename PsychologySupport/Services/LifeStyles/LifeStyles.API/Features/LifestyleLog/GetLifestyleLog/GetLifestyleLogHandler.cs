using BuildingBlocks.CQRS;
using LifeStyles.API.Data;
using LifeStyles.API.Dtos;
using LifeStyles.API.Exceptions;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace LifeStyles.API.Features.LifestyleLog.GetLifestyleLog;

public record GetLifestyleLogQuery(Guid PatientProfileId) : IQuery<GetLifestyleLogResult>;

public record GetLifestyleLogResult(LifestyleLogDto LifestyleLog);

public class GetLifestyleLogHandler(LifeStylesDbContext context)
    : IQueryHandler<GetLifestyleLogQuery, GetLifestyleLogResult>
{
    public async Task<GetLifestyleLogResult> Handle(GetLifestyleLogQuery request, CancellationToken cancellationToken)
    {
        var latestLog = await context.LifestyleLogs
            .Where(x => x.PatientProfileId == request.PatientProfileId)
            .OrderByDescending(x => x.LogDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestLog is null)
            throw new LifeStylesNotFoundException("Lifestyle Log", request.PatientProfileId);

        var dto = latestLog.Adapt<LifestyleLogDto>(); 

        return new GetLifestyleLogResult(dto);
    }
}
