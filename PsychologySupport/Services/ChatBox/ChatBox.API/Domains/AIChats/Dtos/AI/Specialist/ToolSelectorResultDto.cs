namespace ChatBox.API.Domains.AIChats.Dtos.AI.Specialist;

public sealed class ToolSelectorResultDto
{
    /// <summary>
    /// Quyết định về tool (DÙNG DTO ToolCallBlock CÓ SẴN)
    /// </summary>
    public ToolCallBlock ToolCall { get; set; } = new();

    /// <summary>
    /// Quyết định về CTA (DÙNG DTO CtaBlock CÓ SẴN)
    /// (Có thể null nếu tool không cần CTA)
    /// </summary>
    public CtaBlock? Cta { get; set; }
}