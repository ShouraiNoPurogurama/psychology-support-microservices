using BuildingBlocks.DDD;

namespace ChatBox.API.Models;

public class AIChatSession : DomainEventContainer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public string Name { get; set; } = "Đoạn chat mới";
    public bool? IsActive { get; set; } = true;
    public bool IsLegacy { get; set; } = false;

    // Thêm cho mục đích tóm tắt
    public string? Summarization { get; set; }  
    public DateTimeOffset? LastSummarizedAt { get; set; } 
    public int? LastSummarizedIndex { get; set; }
    public PersonaSnapshot? PersonaSnapshot { get; set; }
    
    public string? PersonaSnapshotJson
    {
        get => PersonaSnapshot?.ToJson();
        set => PersonaSnapshot = value == null ? null : PersonaSnapshot.FromJson(value);
    }
}
public static class PersonaSnapshotExtensions
{
    /// <summary>
    /// Chuyển đổi PersonaSnapshot thành đoạn prompt tiếng Việt, thân thiện cho AI.
    /// </summary>
    public static string ToPromptText(this PersonaSnapshot? snapshot)
    {
        if (snapshot == null)
            return "Chưa có thông tin hồ sơ người dùng (Persona).";

        return $"""
                [System]
                Thông tin người dùng hiện tại (Persona):
                - Nghề nghiệp: {snapshot.JobTitle}
                
                
                """;
    }
}


