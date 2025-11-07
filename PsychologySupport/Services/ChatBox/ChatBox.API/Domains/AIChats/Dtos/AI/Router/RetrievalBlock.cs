using Newtonsoft.Json;

namespace ChatBox.API.Domains.AIChats.Dtos.AI.Router;

public sealed class RetrievalBlock
{
    // TRUE nếu cần augment (personal memory / team knowledge)
    [JsonProperty("needed")]
    public bool Needed { get; set; }

    [JsonProperty("scopes")]
    public RetrievalScopes Scopes { get; set; } = new();

    // Optional gợi ý từ khóa nếu router muốn đề xuất
    [JsonProperty("hints")]
    public RetrievalHints? Hints { get; set; }
}