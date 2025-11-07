using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI.Router;

public sealed class GuidanceBlock
{
    // Một dòng chỉ dẫn cho Emo (không phải câu trả lời)
    [JsonProperty("emo_instruction")]
    public string EmoInstruction { get; set; } = string.Empty;
}