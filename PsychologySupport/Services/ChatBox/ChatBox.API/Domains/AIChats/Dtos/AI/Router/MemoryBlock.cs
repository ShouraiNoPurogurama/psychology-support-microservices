using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI.Router;

public sealed class MemoryBlock
{
    [JsonProperty("save")]
    public SaveBlock Save { get; set; } = new();
}