using System.Text.Json;
using System.Text.Json.Serialization;
using BuildingBlocks.Utils;
using Chatbox.API.Protos;
using UserMemory.API.Shared.Dtos;
using UserMemory.API.Shared.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace UserMemory.API.Shared.Services;

/// <summary>
/// Tạo final prompt sinh ảnh (1 roundtrip text):
/// - Gọi Chat Service (gRPC) lấy Persona + Summary
/// - Gọi Gemini (text) 1 lần để sinh "Filler"
/// - Bubble text để image agent tự tạo trong lúc render
/// - Ghép SubjectLock + Style + Negative + Base + Filler + BubbleAutoInstruction
/// </summary>
public class ChatAndAiPromptGenerator(
    ChatboxService.ChatboxServiceClient grpcClient,
    IGeminiService geminiService,
    ILogger<ChatAndAiPromptGenerator> logger)
    : IRewardPromptGenerator
{
    // ===== (A) BASE =====
    private const string PromptBase = """
    A highly detailed 3D illustration featuring the subject from the attached image as the main character, radiating a soft, warm yellowish glow (like a comforting silicone nightlight).
    Center composition on this subject; surrounding elements are supportive only.
    """;

    // ===== (B) SUBJECT LOCK =====
    private const string SubjectLock = """
    Use the attached image as the primary subject.
    - Preserve the character’s identity, silhouette, proportions, face geometry, costume cues, and color palette.
    - The attached character is the only protagonist. Do not replace, morph, or transform them into animals.
    - Keep camera framing centered on this subject.
    """;

    // ===== (C) STYLE =====
    private const string PromptStyle = """
    STYLE (transfer-only): Masterpiece 3D animation inspired by Pixar/DreamWorks; cinematic composition; hyper-realistic soft textures (silicone/plastic glow); dramatic volumetric lighting; shallow depth of field; 8K clarity.
    Do not alter the subject’s identity; apply style only.
    """;

    // ===== (D) NEGATIVE =====
    private const string NegativePrompt = """
    NEGATIVE:
    - No cats/dogs/animals or new characters unless provided.
    - No morphing, replacement, or cluttered background.
    - Avoid text overlays, watermark, or logos.
    """;

    // ===== (E) FILLER Template =====
    private const string FillerGenerationPromptTemplate = """
    Generate the **FILLER** (ACTION/SETTING/DETAILS) for the same single subject from the attached image.
    - Keep the subject as-is (no species morphing or replacement).
    - Do NOT mention chats, chatbots, or dialogues.

    DATA:
    {
      "age": %AGE%,
      "gender": "%GENDER%",
      "job": "%JOB%",
      "scene_cue": "%SCENE_CUE%"
    }

    FILLER (about the attached subject):
    "The subject (same as in the attached image) is a [age]-year-old [gender] [job].
    ACTION/SETTING: Reflect 'scene_cue' with a clear action and location (no dialogue).
    EXPRESSION: Face and posture match the mood from 'scene_cue'.
    SCENE: Environment and atmosphere align with 'job' and 'scene_cue'.
    DETAILS: Include 3–5 small, job-related props matching 'scene_cue' (e.g., laptop glow, coffee mug, notes).
    LIGHTING: Soft but dramatic; highlight the subject’s warm silicone-like glow; keep background clean."

    Return only the FILLER text.
    """;

    // ===== (F) Bubble Instruction (image agent tự tạo) =====
    private const string BubbleAutoInstructionTemplate = """
SPEECH BUBBLE (overlay, auto-content):
- Add a small, soft, semi-transparent white bubble near the subject’s head.
- Keep it very short (under 12 words) and emotionally natural.
- The text should feel like the subject’s *inner thought* or *brief emotional reaction* that matches the scene.
- Personalize the tone to fit the subject’s current mood and context 
  (e.g., quiet reflection, gentle self-encouragement, subtle fatigue, or emotional sigh).
- Avoid clichés, quotes, emojis, or long sentences.
""";

    // ===== MAIN FUNCTION =====
    public async Task<RewardGenerationDataDto> PrepareGenerationDataAsync(Guid aliasId, Guid rewardId, Guid chatSessionId,
        CancellationToken ct)
    {
        logger.LogInformation("Generating prompt for RewardId: {RewardId}, AliasId: {AliasId}", rewardId, aliasId);

        var chatData = await GetChatDataAsync(aliasId, chatSessionId, ct);

        // Build scene cue from metadata
        var sceneCue = BuildSceneCue(chatData.Summary.Metadata);

        var fillerPrompt = FillerGenerationPromptTemplate
            .Replace("%AGE%", TimeUtils.GetAgeFromDateTimeOffsetStr(chatData.Persona.BirthDate).ToString())
            .Replace("%GENDER%", chatData.Persona.Gender ?? "")
            .Replace("%JOB%", chatData.Persona.JobTitle ?? "")
            .Replace("%SCENE_CUE%", sceneCue);

        logger.LogDebug("Calling Gemini (text) to generate FILLER for RewardId: {RewardId}", rewardId);
        var generatedFiller = await geminiService.GenerateTextAsync(fillerPrompt, ct);

        var finalPrompt =
            $@"{SubjectLock}
{PromptStyle}
{NegativePrompt}

{PromptBase}

{generatedFiller}

{BubbleAutoInstructionTemplate}";

        var storedFiller = $"{generatedFiller}\n\n[SPEECH_BUBBLE]\n(Auto by image agent)";

        return new RewardGenerationDataDto(
            UserId: chatData.UserId,
            FinalPrompt: finalPrompt,
            PromptBase: PromptBase,
            PromptFiller: storedFiller
        );
    }

    // ===== Helper: Combine metadata into a unified scene cue =====
    private static string BuildSceneCue(ChatMetadata? m)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(m?.ImageContext))
            parts.Add(m.ImageContext.Trim());

        if (!string.IsNullOrWhiteSpace(m?.EmotionContext))
            parts.Add($"cảm xúc chủ đạo: {m.EmotionContext.Trim()}");

        if (!string.IsNullOrWhiteSpace(m?.Topic))
            parts.Add($"ngữ cảnh: {m.Topic.Trim()}");

        if (parts.Count == 0)
            return "ngồi bình thản trong không gian trung tính, ánh sáng dịu";

        var cue = string.Join(" | ", parts);
        return cue.Length > 240 ? cue[..240] : cue;
    }

    // ===== gRPC =====
    private async Task<(PersonaSnapshot Persona, ChatSummary Summary, Guid UserId)> GetChatDataAsync(Guid aliasId, Guid chatSessionId,
        CancellationToken ct)
    {
        try
        {
            var response = await grpcClient.GetDailySummaryAsync(new GetDailySummaryRequest
            {
                AliasId = aliasId.ToString(),
                ChatSessionId = chatSessionId.ToString()
            }, cancellationToken: ct);

            if (string.IsNullOrEmpty(response.PersonaSnapshotJson) || string.IsNullOrEmpty(response.SummarizationJson))
                throw new InvalidOperationException("Missing required chat data to generate prompt.");

            var persona = JsonSerializer.Deserialize<PersonaSnapshot>(response.PersonaSnapshotJson) ?? new PersonaSnapshot();
            var userId = Guid.Parse(response.UserId);

            var jsonSummaries = response.SummarizationJson.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);
            var latestSummaryJson = jsonSummaries.LastOrDefault()?.Trim();

            ChatSummaryJson summaryJson;
            try
            {
                summaryJson = JsonSerializer.Deserialize<ChatSummaryJson>(latestSummaryJson!) ?? new ChatSummaryJson();
            }
            catch (JsonException ex)
            {
                logger.LogError(ex, "Failed to parse summary JSON for AliasId: {AliasId}", aliasId);
                throw;
            }

            var summary = new ChatSummary(
                Current: summaryJson.Current ?? "",
                Persist: summaryJson.Persist ?? "",
                Metadata: summaryJson.Metadata,
                CreatedAt: summaryJson.CreatedAt
            );

            return (persona, summary, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch or parse chat data via gRPC for AliasId: {AliasId}", aliasId);
            throw;
        }
    }

    // ===== DTOs =====
    private class PersonaSnapshot
    {
        public string Gender { get; set; } = "";
        public string BirthDate { get; set; } = "";
        public string JobTitle { get; set; } = "";
    }

    private class ChatSummaryJson
    {
        [JsonPropertyName("current")]
        public string? Current { get; set; }

        [JsonPropertyName("persist")]
        public string? Persist { get; set; }

        [JsonPropertyName("metadata")]
        public ChatMetadata? Metadata { get; set; }

        [JsonPropertyName("created_at")]
        public DateTimeOffset? CreatedAt { get; set; }
    }

    private class ChatMetadata
    {
        [JsonPropertyName("emotionContext")]
        public string? EmotionContext { get; set; }

        [JsonPropertyName("topic")]
        public string? Topic { get; set; }

        [JsonPropertyName("imageContext")]
        public string? ImageContext { get; set; }
    }

    private record ChatSummary(
        string Current,
        string Persist,
        ChatMetadata? Metadata,
        DateTimeOffset? CreatedAt);
}
