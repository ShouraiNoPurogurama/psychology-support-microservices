using BuildingBlocks.CQRS;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Models;
using UserMemory.API.Shared.Enums;
using UserMemory.API.Shared.Services.Contracts;

namespace UserMemory.API.Domains.UserMemories.Features.SaveMemory;

public record SaveMemoryCommand(Guid AliasId, Guid SessionId, string Summary, List<string> Tags, bool SaveNeeded = false) : ICommand<SaveMemoryResult>;
public record SaveMemoryResult(Guid MemoryId);

public class SaveMemoryHandler(
    UserMemoryDbContext dbContext,
    IEmbeddingService embeddingService
) : ICommandHandler<SaveMemoryCommand, SaveMemoryResult>
{
    private const int AnyMessagePoints = 60;
    private const int SaveNeededPoints = 120;
    private const int EmotionOrPersonalPoints = 50;

   public async Task<SaveMemoryResult> Handle(SaveMemoryCommand request, CancellationToken cancellationToken)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        // (1) Save memory
        var row = await embeddingService.UpsertMemoryAsync(
            aliasId: request.AliasId,
            summary: request.Summary,
            request.Tags.ToArray(),
            cancellationToken);

        // (2) Tính điểm
        var points = CalculateProgressPoints(request.SaveNeeded, request.Tags);
        if (points == 0)
        {
            await tx.CommitAsync(cancellationToken);
            return new SaveMemoryResult(row.Id);
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // (3a) Ghi log chi tiết cho Session (như cũ, rất tốt để debug)
        // Dùng FirstOrDefault + Add/Update
        var sessionDp = await dbContext.SessionDailyProgresses
            .FirstOrDefaultAsync(x => x.SessionId == request.SessionId && x.ProgressDate == today, cancellationToken);
        
        if (sessionDp is null)
        {
            dbContext.SessionDailyProgresses.Add(new SessionDailyProgress {
                SessionId = request.SessionId,
                AliasId = request.AliasId,
                ProgressDate = today,
                ProgressPoints = points,
                LastIncrement = points
            });
        }
        else
        {
            sessionDp.ProgressPoints += points;
            sessionDp.LastIncrement = points;
        }
        
        // (3b) Cập nhật Bảng Tóm Tắt (MỚI)
        // Dùng ExecuteUpdateAsync để "Upsert"
        var rowsAffected = await dbContext.SessionDailyProgresses
            .Where(s => s.AliasId == request.AliasId && s.ProgressDate == today)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(s => s.ProgressPoints, s => s.ProgressPoints + points),
            cancellationToken);

        // Nếu update 0 hàng (chưa có record tóm tắt) -> "Seed" nó
        if (rowsAffected == 0)
        {
            dbContext.AliasDailySummaries.Add(new AliasDailySummary
            {
                AliasId = request.AliasId,
                Date = today,
                RewardClaimCount = 0 // Mặc định 0, vì đây là luồng điểm tiến trình
            });
        }

        // SaveChanges sẽ lưu cả 2 bảng (SessionDailyProgress + AliasDailySummary)
        await dbContext.SaveChangesAsync(cancellationToken); 
        await tx.CommitAsync(cancellationToken);

        return new SaveMemoryResult(row.Id);
    }

    private static int CalculateProgressPoints(bool saveNeeded, List<string>? tags)
    {
        var sum = AnyMessagePoints;

        if (saveNeeded) sum += SaveNeededPoints;

        if (tags is { Count: > 0 } && HasEmotionOrPersonalLife(tags))
            sum += EmotionOrPersonalPoints;

        return sum;
    }

    /// <summary>
    /// Rule: có tag bắt đầu bằng "emotion_" hoặc "personal_life_".
    /// Ngoài ra, nếu bạn muốn “personal life” bao trùm: Topic_Family/Health/Hobby/Travel
    /// hoặc Relationship_* (trừ Colleague) thì bật nhánh mở rộng ở dưới.
    /// </summary>
    private static bool HasEmotionOrPersonalLife(IEnumerable<string> tags)
    {
        foreach (var raw in tags)
        {
            if (string.IsNullOrWhiteSpace(raw)) continue;
            var t = raw.Trim();

            // 1) Theo string-prefix từ Router
            if (t.StartsWith("emotion_", StringComparison.OrdinalIgnoreCase)) return true;
            if (t.StartsWith("personal_life_", StringComparison.OrdinalIgnoreCase)) return true;

            // 2) (Tuỳ chọn) Map sang enum & suy luận "đời sống cá nhân"
            //    Bật đoạn dưới nếu bạn muốn nhận diện thêm từ các enum bạn có:
            /*
            if (TryMapEmotionTag(t, out _)) return true;
            if (TryMapTopicTag(t, out var topic))
            {
                if (topic is TopicTag.Topic_Family or TopicTag.Topic_Health or TopicTag.Topic_Hobby or TopicTag.Topic_Travel)
                    return true;
            }
            if (TryMapRelationshipTag(t, out var rel))
            {
                if (rel != RelationshipTag.Relationship_Colleague) // coi mọi mối quan hệ ngoài đồng nghiệp là personal
                    return true;
            }
            */
        }
        return false;
    }

    // ====== (Tuỳ chọn) Helpers map string -> enum (nếu muốn bật logic mở rộng) ======
    // Bạn có thể để riêng ra 1 static class TagMapping nếu thích.

    private static bool TryMapEmotionTag(string tag, out EmotionTag value)
        => Enum.TryParse(NormalizeEnumName(tag, "emotion_"), ignoreCase: true, out value);

    private static bool TryMapRelationshipTag(string tag, out UserMemory.API.Shared.Enums.RelationshipTag value)
        => Enum.TryParse(NormalizeEnumName(tag, "relationship_"), ignoreCase: true, out value);

    private static bool TryMapTopicTag(string tag, out UserMemory.API.Shared.Enums.TopicTag value)
        => Enum.TryParse(NormalizeEnumName(tag, "topic_"), ignoreCase: true, out value);

    /// <summary>
    /// "emotion_tired" -> "Emotion_Tired"
    /// "Topic_Family"  -> "Topic_Family" (giữ nguyên nếu đã đúng)
    /// </summary>
    private static string NormalizeEnumName(string input, string knownPrefix)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        var s = input.Trim();

        // nếu đã đúng PascalCase Enum rồi thì trả lại
        if (s.Contains('_') && char.IsUpper(s[0])) return s;

        // ép về lower-case, chuẩn hoá prefix
        s = s.Replace('-', '_');
        if (!s.Contains('_') && s.StartsWith(knownPrefix, StringComparison.OrdinalIgnoreCase) == false)
            s = $"{knownPrefix}{s}";

        // tách theo underscore và PascalCase từng mảnh
        var parts = s.Split('_', StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            var p = parts[i].ToLowerInvariant();
            parts[i] = char.ToUpperInvariant(p[0]) + p[1..];
        }
        return string.Join('_', parts);
    }
}