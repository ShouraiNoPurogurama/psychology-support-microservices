﻿namespace Translation.API.Dtos.Gemini;

public record GeminiGenerationConfigDto(
    // [JsonProperty("temperature")]
    // double Temperature = 1.0,
    // [JsonProperty("topP")]
    // double TopP = 0.95,
    // [JsonProperty("maxOutputTokens")]
    // int MaxOutputTokens = 8192,
    object ResponseSchema,
    string ResponseMimeType = "application/json"
);
