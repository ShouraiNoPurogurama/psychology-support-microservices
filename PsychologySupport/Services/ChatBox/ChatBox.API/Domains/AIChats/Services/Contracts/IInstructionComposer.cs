using ChatBox.API.Domains.AIChats.Enums;

namespace ChatBox.API.Domains.AIChats.Services.Contracts;

public interface IInstructionComposer
{
    string Compose(
        RouterIntent intent,
        RouterToolType? toolType,
        string basePersona,        // config.Value.SystemInstruction (persona mặc định)
        string? routerGuidance,    // decision.Guidance?.EmoInstruction
        string? extraGuards = null // guardrails chung (tuỳ chọn)
    );
}