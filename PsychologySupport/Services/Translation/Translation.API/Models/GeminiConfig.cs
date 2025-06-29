namespace Translation.API.Models;

public class GeminiConfig
{
    public string ProjectId { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string EndpointId { get; set; } = string.Empty;
    public string SystemInstruction { get; set; } = string.Empty;
    
    public string SummaryInstruction { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}