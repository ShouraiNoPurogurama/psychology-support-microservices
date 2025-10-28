using BuildingBlocks.Constants;
using Microsoft.EntityFrameworkCore;
using UserMemory.API.Data;
using UserMemory.API.Models;
using UserMemory.API.Shared.Enums;
using UserMemory.API.Shared.Services.Contracts;
using UserMemory.API.Shared.Utils;

namespace UserMemory.API.Shared.Services;

public class TagSyncService : ITagSyncService
{
    private readonly UserMemoryDbContext _db;
    private readonly Dictionary<string, string> _renameMap;

    public TagSyncService(UserMemoryDbContext db)
    {
        _db = db;
        _renameMap = new()
        {
            // ví dụ: {"Topic_HealthCare","Topic_Health"} // khi đổi tên enum
        };
    }

    public async Task SyncAsync(CancellationToken ct = default)
    {
        var wanted = new List<(string code, string name, string category)>();

        // Topic
        wanted.AddRange(Enum.GetValues<TopicTag>()
            .Select(x => (x.ToString(), ToDisplay(x), "Topic")));

        // Emotion
        wanted.AddRange(Enum.GetValues<EmotionTag>()
            .Select(x => (x.ToString(), ToDisplay(x), "Emotion")));

        // Relationship
        wanted.AddRange(Enum.GetValues<RelationshipTag>()
            .Select(x => (x.ToString(), ToDisplay(x), "Relationship")));

        var wantedCodes = wanted.Select(x => x.code).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var tags = await _db.MemoryTags.ToListAsync(ct);

        // Rename (optional)
        foreach (var (oldCode, newCode) in _renameMap)
        {
            var exist = tags.FirstOrDefault(t => t.Code == oldCode);
            if (exist != null)
            {
                exist.Code = newCode;
                exist.Category = GetCategoryFromCode(newCode);
            }
        }

        // Insert missing
        foreach (var w in wanted)
        {
            if (tags.All(t => !t.Code.Equals(w.code, StringComparison.Ordinal)))
            {
                _db.MemoryTags.Add(new MemoryTag
                {
                    Id = Guid.NewGuid(),
                    Code = w.code,
                    Name = w.name,
                    Category = w.category,
                    IsActive = true,
                    CreatedAt = DateTimeOffset.UtcNow,
                    CreatedBy = SystemActors.SystemUUID.ToString()
                });
            }
        }

        // Deactivate tags không còn trong enum
        var toDeactivate = tags.Where(t => !wantedCodes.Contains(t.Code));
        foreach (var t in toDeactivate) t.IsActive = false;

        await _db.SaveChangesAsync(ct);
    }

    private static string GetCategoryFromCode(string code)
        => code.StartsWith("Topic_") ? "Topic"
            : code.StartsWith("Emotion_") ? "Emotion"
            : code.StartsWith("Relationship_") ? "Relationship"
            : "Other";

    private static string ToDisplay(TopicTag t) => t switch
    {
        TopicTag.Topic_Food => "Food",
        TopicTag.Topic_Work => "Work",
        TopicTag.Topic_Study => "Study",
        TopicTag.Topic_Family => "Family",
        TopicTag.Topic_Health => "Health",
        TopicTag.Topic_Travel => "Travel",
        TopicTag.Topic_Hobby => "Hobby",
        TopicTag.Topic_Finance => "Finance",
        TopicTag.Topic_Event => "Event",
        TopicTag.Topic_Reflection => "Self-reflection",
        _ => t.ToString()
    };

    private static string ToDisplay(EmotionTag t) => t switch
    {
        EmotionTag.Emotion_Happy => "Happy",
        EmotionTag.Emotion_Excited => "Excited",
        EmotionTag.Emotion_Grateful => "Grateful",
        EmotionTag.Emotion_Love => "Love",
        EmotionTag.Emotion_Proud => "Proud",
        EmotionTag.Emotion_Sad => "Sad",
        EmotionTag.Emotion_Angry => "Angry",
        EmotionTag.Emotion_Anxious => "Anxious",
        EmotionTag.Emotion_Stressed => "Stressed",
        EmotionTag.Emotion_Tired => "Tired",
        EmotionTag.Emotion_Lonely => "Lonely",
        EmotionTag.Emotion_Guilty => "Guilty",
        EmotionTag.Emotion_Surprised => "Surprised",
        EmotionTag.Emotion_Confused => "Confused",
        _ => t.ToString()
    };

    private static string ToDisplay(RelationshipTag t) => t switch
    {
        RelationshipTag.Relationship_Partner => "Partner / Spouse",
        RelationshipTag.Relationship_Friend => "Friend",
        RelationshipTag.Relationship_Family => "Family",
        RelationshipTag.Relationship_Colleague => "Colleague",
        RelationshipTag.Relationship_Self => "Self",
        _ => t.ToString()
    };
}