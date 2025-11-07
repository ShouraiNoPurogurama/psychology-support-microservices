using ChatBox.API.Domains.AIChats.Enums;

namespace ChatBox.API.Domains.AIChats.Dtos.AI.Router;

public record RouterSnapshot(
    RouterIntent Intent,
    bool RetrievalNeeded,
    bool UsePersonalMemory,
    bool UseTeamKnowledge,
    bool SaveNeeded,
    MemoryPayload? MemoryPayload,
    string Instruction,
    RouterToolType? ToolCallType = null
);