using BuildingBlocks.Data.Common;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Pii;
using BuildingBlocks.Utils;
using Npgsql;
using Profile.API.Data.Pii;
using Profile.API.Domains.Pii.Dtos;
using Profile.API.Models.Pii;

namespace Profile.API.Domains.Pii.Features.EnsureSubjectRef;

public record SeedSubjectRefCommand(Guid UserId, PersonSeedDto Seed) : ICommand<SeedSubjectRefResult>;
public record SeedSubjectRefResult(bool IsSuccess);

public class EnsureSubjectRefHandler(
    PiiDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ILogger<EnsureSubjectRefHandler> logger)
    : ICommandHandler<SeedSubjectRefCommand, SeedSubjectRefResult>
{
    public async Task<SeedSubjectRefResult> Handle(SeedSubjectRefCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var seed   = request.Seed;

        ValidationBuilder.Create()
            .When(() => userId == Guid.Empty)
                .WithErrorCode("INVALID_USER_ID")
                .WithMessage("User Id không được để trống.")
            .When(() => seed.SubjectRef == Guid.Empty)
                .WithErrorCode("INVALID_SUBJECT_REF")
                .WithMessage("SubjectRef không hợp lệ.")
            .ThrowIfInvalid();

        var existed = await dbContext.PersonProfiles.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => p.SubjectRef)
            .FirstOrDefaultAsync(cancellationToken);

        if (existed != Guid.Empty)
            return new SeedSubjectRefResult(false);

        var subjectRef = seed.SubjectRef;

        ContactInfo contactInfo = ContactInfo.Of(null, seed.Email, seed.PhoneNumber);

        var personProfile = PersonProfile.SeedPending(
            subjectRef: subjectRef,
            userId    : userId,
            fullName  : seed.FullName,              
            contactInfo: contactInfo            
        );

        dbContext.PersonProfiles.Add(personProfile);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);

            //mit sự kiện “đã seed xong SubjectRef” (không phải CreatedActive)
            await publishEndpoint.Publish(new PiiCreatedIntegrationEvent(personProfile.SubjectRef), cancellationToken);

            return new SeedSubjectRefResult(true);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            //Thua race → đọc winner và trả về (không merge)
            logger.LogDebug(ex, "Unique(user_id) violated; reading winner");

            var winner = await dbContext.PersonProfiles.AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => p.SubjectRef)
                .FirstOrDefaultAsync(cancellationToken);

            if (winner == Guid.Empty) throw; //cực hiếm, để middleware retry

            return new SeedSubjectRefResult(false);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
}
