using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI.Router;

public sealed class SaveBlock
{
    [JsonProperty("needed")]
    public bool Needed { get; set; }

    // BẮT BUỘC có nếu Needed = true
    [JsonProperty("payload")]
    public MemoryPayload? Payload { get; set; }
}