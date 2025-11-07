using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI.Router
{
    public sealed class RouterDecisionDto
    {
        [JsonProperty("route")]
        public RouteBlock Route { get; set; } = new();

        [JsonProperty("guidance")]
        public GuidanceBlock Guidance { get; set; } = new();

        [JsonProperty("retrieval")]
        public RetrievalBlock Retrieval { get; set; } = new();

        [JsonProperty("memory")]
        public MemoryBlock Memory { get; set; } = new();
        
    }

    // Thay cho MemoryToSaveDto cũ
}
