using BuildingBlocks.Data.Common;
using BuildingBlocks.Messaging.Events.IntegrationEvents.Pii;
using Npgsql;
using Profile.API.Data.Pii;
using Profile.API.Domains.Pii.Dtos;
using Profile.API.Models.Pii;

namespace Profile.API.Domains.Pii.Features.EnsureSubjectRef;

public record EnsureSubjectRefCommand(Guid UserId, PersonSeedDto? Seed) : ICommand<EnsureSubjectRefResult>;

public record EnsureSubjectRefResult(Guid SubjectRef);

public class EnsureSubjectRefHandler(PiiDbContext dbContext, IPublishEndpoint publishEndpoint, ILogger<EnsureSubjectRefHandler> logger)
    : ICommandHandler<EnsureSubjectRefCommand, EnsureSubjectRefResult>
{
    public async Task<EnsureSubjectRefResult> Handle(EnsureSubjectRefCommand request, CancellationToken cancellationToken)
    {
        var userId = request.UserId;
        var seed = request.Seed;

        var existed = await dbContext.PersonProfiles.AsNoTracking()
            .Where(p => p.UserId == userId)
            .Select(p => new
            {
                p.SubjectRef
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (existed is not null)
        {
            if (seed is not null)
                await MergeIfNeededAsync(existed.SubjectRef, seed, cancellationToken);

            return new EnsureSubjectRefResult(existed.SubjectRef);
        }

        //Nếu chưa có person profile thì tạo mới
        var subjectRef = Guid.NewGuid();

        var personProfile = PersonProfile.Create(
            subjectRef: subjectRef,
            userId: userId,
            fullName: seed?.FullName,
            gender: seed?.Gender,
            birthDate: seed?.BirthDate,
            contactInfo: seed?.ContactInfo ?? new ContactInfo());
        
        dbContext.PersonProfiles.Add(personProfile);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            
            var piiCreatedIntegrationEvent = new PiiCreatedIntegrationEvent(personProfile.SubjectRef);
            await publishEndpoint.Publish(piiCreatedIntegrationEvent, cancellationToken);
            
            return new EnsureSubjectRefResult(subjectRef);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            //Thua race: bên khác đã chèn trước
            logger.LogDebug(ex, "Unique(user_id) violated; reading winner");

            var winner = await dbContext.PersonProfiles
                .AsNoTracking()
                .Where(p => p.UserId == userId)
                .Select(p => p.SubjectRef)
                .FirstOrDefaultAsync(cancellationToken);

            if (winner == Guid.Empty)
                throw; //cực hiếm: đọc chưa thấy, để bubble lên retry

            //Merge nhẹ nếu có seed
            if (seed is not null)
                await MergeIfNeededAsync(winner, seed, cancellationToken);

            return new EnsureSubjectRefResult(winner);
        }
    }

    private async Task MergeIfNeededAsync(Guid subjectRef, PersonSeedDto seedDto, CancellationToken ct)
    {
        var entity = await dbContext.PersonProfiles.FirstOrDefaultAsync(p => p.SubjectRef == subjectRef, ct);
        if (entity is null) return;

        bool changed = false;

        //Merge kiểu “điền chỗ trống” (chỉ ghi nếu đang null/empty)
        if (!string.IsNullOrWhiteSpace(seedDto.FullName) && string.IsNullOrWhiteSpace(entity.FullName?.Value))
        {
            entity.Rename(seedDto.FullName);
            changed = true;
        }

        if (entity.Gender == default)
        {
            entity.ChangeGender(seedDto.Gender);
            changed = true;
        }

        if (entity.BirthDate is null)
        {
            entity.ChangeBirthDate(seedDto.BirthDate);
            changed = true;
        }

        var seedCI = seedDto.ContactInfo;
        var entityCI = entity.ContactInfo;

        bool contactChanged = false;

        if (!string.IsNullOrWhiteSpace(seedCI.Email) && string.IsNullOrWhiteSpace(entityCI.Email))
        {
            entityCI.Email = seedCI.Email;
            contactChanged = true;
        }

        if (!string.IsNullOrWhiteSpace(seedCI.PhoneNumber) && string.IsNullOrWhiteSpace(entityCI.PhoneNumber))
        {
            entityCI.PhoneNumber = seedCI.PhoneNumber;
            contactChanged = true;
        }

        if (!string.IsNullOrWhiteSpace(seedCI.Address) && string.IsNullOrWhiteSpace(entityCI.Address))
        {
            entityCI.Address = seedCI.Address;
            contactChanged = true;
        }

        if (contactChanged)
        {
            entity.UpdateContact(entityCI);
            changed = true;
        }

        if (changed)
        {
            await dbContext.SaveChangesAsync(ct);
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
}