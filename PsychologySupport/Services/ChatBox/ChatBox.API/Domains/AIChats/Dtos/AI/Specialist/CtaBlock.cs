using ChatBox.API.Domains.AIChats.Enums;
using Newtonsoft.Json.Linq;

public sealed class CtaBlock
{
    public bool Needed { get; set; }
    public string? Title { get; set; } // Optional
    public string? ResourceKey { get; set; } // Ví dụ: "DASS21_FE_LINK"
    public string? NavigateUrl { get; set; } // URL mà ToolSelector "lấp liếm" điền vào}
}


// Dtos/AI/Router/ToolCallBlock.cs
public sealed class ToolCallBlock
{
    public bool Needed { get; set; }                 // true khi route.intent = TOOL_CALLING
    public RouterToolType Type { get; set; }         // ví dụ: DASS21_TEST
    public string? ResourceKey { get; set; }         // ví dụ: "DASS21_FE_LINK"
    public JObject? Hints { get; set; }              // optional gợi ý mờ
}
