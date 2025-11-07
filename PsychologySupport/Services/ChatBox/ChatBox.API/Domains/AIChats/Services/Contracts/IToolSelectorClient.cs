using ChatBox.API.Domains.AIChats.Dtos.AI;
using ChatBox.API.Domains.AIChats.Dtos.AI.Specialist;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IToolSelectorClient
{
    /// <summary>
    /// Được gọi khi Router chính xác định intent là TOOL_CALLING.
    /// Service này sẽ quyết định CỤ THỂ tool nào (ToolCallBlock) và CTA (CtaBlock) đi kèm.
    /// </summary>
    Task<ToolSelectorResultDto?> SelectToolAsync(
        string userMessage,
        List<HistoryMessage> historyMessages,
        CancellationToken ct = default);
}