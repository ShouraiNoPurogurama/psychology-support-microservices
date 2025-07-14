using BuildingBlocks.DDD;
using ChatBox.API.Data.Migrations;

namespace ChatBox.API.Models;

public class AIChatSession : DomainEventContainer
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedDate { get; set; }
    public string Name { get; set; } = "Đoạn chat mới";
    public bool? IsActive { get; set; } = true;

    // Thêm cho mục đích tóm tắt
    public string? Summarization { get; set; }  
    public DateTime? LastSummarizedAt { get; set; } 
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
                <System>
                Thông tin người dùng hiện tại (Persona):
                - Họ tên: {snapshot.FullName}
                - Giới tính: {snapshot.Gender}
                - Ngày sinh: {snapshot.BirthDate}
                - Nghề nghiệp: {snapshot.JobTitle}
                - Trình độ học vấn: {snapshot.EducationLevel}
                - Ngành nghề: {snapshot.IndustryName}
                - Tính cách nổi bật: {snapshot.PersonalityTraits}
                - Tiền sử dị ứng: {snapshot.Allergies}
                </System>

                """;
    }
}


