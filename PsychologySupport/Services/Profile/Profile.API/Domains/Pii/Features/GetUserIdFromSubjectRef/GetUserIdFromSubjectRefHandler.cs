using Profile.API.Data.Pii;

namespace Profile.API.Domains.Pii.Features.GetUserIdFromSubjectRef;

public record GetUserIdFromSubjectRefCommand(Guid SubjectRef) : IQuery<GetUserIdFromSubjectRefResult>;

public record GetUserIdFromSubjectRefResult(Guid UserId);

public class GetUserIdFromSubjectRefHandler(PiiDbContext dbContext) : IQueryHandler<GetUserIdFromSubjectRefCommand, GetUserIdFromSubjectRefResult>
{
    public async Task<GetUserIdFromSubjectRefResult> Handle(GetUserIdFromSubjectRefCommand request, CancellationToken cancellationToken)
    {
        var userId = await dbContext.PersonProfiles
            .AsNoTracking()
            .Where(x => x.SubjectRef == request.SubjectRef)
            .Select(x => x.UserId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        return new GetUserIdFromSubjectRefResult(userId);
    }
}