using System.Text;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UserMemory.API.Shared.Dtos.Gemini.Embedding;
using UserMemory.API.Shared.Services.Contracts;

namespace UserMemory.API.Shared.Services
{
    public class GeminiClient(
        ILogger<GeminiClient> logger,
        IHttpClientFactory httpClientFactory,
        IOptions<GeminiEmbeddingOptions> embeddingOptions,
        IConfiguration config) : IGeminiClient
    {
        public async Task<float[]> EmbedTextAsync(string text, int? outputDim = null, string? taskType = null, CancellationToken ct = default)
        {
            var apiKey = config["GeminiConfig:ApiKey"] ?? throw new InvalidOperationException("Missing GeminiConfig:ApiKey");
            var model  = config["GeminiConfig:EmbeddingModel"] ?? "models/gemini-embedding-001";

            var http = httpClientFactory.CreateClient();
            var url  = $"https://generativelanguage.googleapis.com/v1beta/models/{TrimModels(model)}:embedContent?key={apiKey}";
            
            var payload = new JObject
            {
                ["task_type"] = taskType ?? embeddingOptions.Value.TaskType,
                ["content"] = new JObject
                {
                    ["parts"] = new JArray(new JObject { ["text"] = text })
                }
            };
            if (outputDim is > 0)
                payload["output_dimensionality"] = outputDim; // snake_case theo docs

            using var req = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
            using var resp = await http.PostAsync(url, req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                logger.LogError("Gemini embedContent failed: {Status} {Body}", resp.StatusCode, body);
                throw new HttpRequestException($"Gemini embed failed: {(int)resp.StatusCode} {resp.ReasonPhrase}");
            }

            // Response single: { "embedding": { "values": [ ... ] } }
            var jo = JObject.Parse(body);
            var values = jo["embedding"]?["values"]?.ToObject<float[]>() ?? Array.Empty<float>();
            return values;
        }

        public async Task<IReadOnlyList<float[]>> EmbedBatchAsync(IEnumerable<string> texts, int? outputDim = null, string? taskType = null, CancellationToken ct = default)
        {
            var apiKey = config["GeminiConfig:ApiKey"] ?? throw new InvalidOperationException("Missing GeminiConfig:ApiKey");
            var model  = config["GeminiConfig:EmbeddingModel"] ?? "models/gemini-embedding-001";

            var http = httpClientFactory.CreateClient();
            var url  = $"https://generativelanguage.googleapis.com/v1beta/models/{TrimModels(model)}:batchEmbedContents?key={apiKey}";

            // requests: [ { content: {...}, output_dimensionality: n }, ... ]
            var requests = texts.Select(t =>
            {
                var reqObj = new JObject
                {
                    ["task_type"] = taskType ?? embeddingOptions.Value.TaskType,
                    ["content"] = new JObject
                    {
                        ["parts"] = new JArray(new JObject { ["text"] = t })
                    }
                };
                if (outputDim is > 0)
                    reqObj["output_dimensionality"] = outputDim; // snake_case
                return reqObj;
            }).ToArray();

            var payload = new JObject { ["requests"] = new JArray(requests) };

            using var req = new StringContent(payload.ToString(Formatting.None), Encoding.UTF8, "application/json");
            using var resp = await http.PostAsync(url, req, ct);
            var body = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                logger.LogError("Gemini batchEmbedContents failed: {Status} {Body}", resp.StatusCode, body);
                throw new HttpRequestException($"Gemini embed failed: {(int)resp.StatusCode} {resp.ReasonPhrase}");
            }

            // Response batch: { "embeddings": [ { "values":[...]} , ... ] }
            var jo = JObject.Parse(body);
            var arr = jo["embeddings"] as JArray ?? [];
            var result = new List<float[]>(arr.Count);
            foreach (var e in arr)
            {
                var vec = e?["values"]?.ToObject<float[]>() ?? Array.Empty<float>();
                result.Add(vec);
            }
            return result;
        }

        private static string TrimModels(string model)
            => model.StartsWith("models/", StringComparison.OrdinalIgnoreCase) ? model["models/".Length..] : model;
    }
}
