using ChatBox.API.Domains.AIChats.Enums;
using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI;

public sealed class RouterDecisionDto
{
    [JsonProperty("intent")]
    public RouterIntent Intent { get; set; }

    [JsonProperty("retrieval_needed")]
    public bool RetrievalNeeded { get; set; }

    [JsonProperty("save_needed")]
    public bool SaveNeeded { get; set; }

    [JsonProperty("memory_to_save")]
    public MemoryToSaveDto? MemoryToSave { get; set; }

    [JsonProperty("emo_instruction")]
    public string? Instruction { get; set; }
}