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
    
    VIETNAMESE AMBIENCE:
    - Set the scene in contemporary Vietnam (everyday urban or campus/workspace vibe).
    - Environmental cues: modest sidewalks, narrow shop fronts, dense power lines, scooters (xe máy), soft tropical foliage, humid air feel.
    - Signage and text (if any) should be in Vietnamese with proper diacritics; use subtle, natural street typography.
    - Props palette: cà phê phin, ly nhựa với ống hút, sổ tay kẹp giấy, bút bi Thiên Long, laptop dán sticker nhỏ, ổ điện chuẩn VN, tiền VND (50K/100K/200K/500K), balo đơn giản, áo mưa mỏng, mũ bảo hiểm treo ở góc khung hình.
    - Color tone: warm-natural, slight filmic softness; avoid overly glossy Western commercial look.
    """;

    // ===== (B) SUBJECT LOCK =====
    private const string SubjectLock = """
                                       Use the attached image as the primary subject.
                                       - Preserve the character’s identity, silhouette, proportions, face geometry, hair, costume cues, and color palette.
                                       - DO NOT change the person’s ethnicity/identity; adapt only environment, props, and ambience.
                                       - Keep camera framing centered on this subject.
                                       """;

    // ===== (C) STYLE =====
    private const string PromptStyle = """
                                       STYLE (transfer-only): Masterpiece 3D animation inspired by Pixar/DreamWorks; cinematic composition; soft, tactile textures (silicone/plastic glow); gentle tropical humidity; shallow depth of field; 8K clarity.
                                       Apply style without altering the subject’s identity; let the Vietnamese ambience define context and prop design language.
                                       """;

    // ===== (D) NEGATIVE =====
    private const string NegativePrompt = """
                                          NEGATIVE:
                                          - No new characters, animals, Western city skylines, or snowy weather.
                                          - No American/European street furniture (yellow cabs, fire hydrants, brownstone houses, iconic Western landmarks).
                                          - No cluttered background, no heavy neon cyberpunk aesthetic, no billboard-level English text.
                                          - Avoid watermark, logos, large English slogans; if text appears, prefer subtle Vietnamese.
                                          """;

    // ===== (E) FILLER Template =====
    private const string FillerGenerationPromptTemplate = """
                                                          Generate the **FILLER** (ACTION/SETTING/DETAILS) for the same single subject from the attached image.
                                                          - Keep the subject as-is (no species morphing or replacement).
                                                          - Do NOT mention chats, chatbots, or dialogues.
                                                          - Set the scene SPECIFICALLY in contemporary Vietnam (urban/campus/workspace/street) with authentic local cues.

                                                          DATA:
                                                          {
                                                            "age": %AGE%,
                                                            "gender": "%GENDER%",
                                                            "job": "%JOB%",
                                                            "scene_cue": "%SCENE_CUE%"
                                                          }

                                                          FILLER (about the attached subject):
                                                          "The subject (same as in the attached image) is a [age]-year-old [gender] [job].
                                                          ACTION/SETTING: Reflect 'scene_cue' in a realistic Vietnamese environment (e.g., small cafe with cà phê phin, shared desk with multi-plugs, scooter-lined sidewalk, campus corridor).
                                                          EXPRESSION: Face and posture match the mood from 'scene_cue'.
                                                          SCENE: Use Vietnamese signage with diacritics when needed; keep it subtle and natural.
                                                          DETAILS: Include 3–5 small, Vietnam-typical props matching 'job' and 'scene_cue' (e.g., laptop with small stickers, notebook + bút bi Thiên Long, ly nhựa với ống hút, ổ điện chuẩn VN, mũ bảo hiểm hoặc áo mưa gấp gọn ở góc khung).
                                                          LIGHTING: Warm, soft, tropical; slight humidity glow; highlight the subject’s silicone-like warm rim light; background clean and uncluttered."

                                                          Return only the FILLER text.
                                                          """;

    // ===== (F) Bubble Instruction (image agent tự tạo) =====
    private const string BubbleAutoInstructionTemplate = """
                                                         SPEECH BUBBLE (overlay, auto-content):
                                                         - Add a small, soft, semi-transparent white bubble near the subject’s head.
                                                         - Keep it very short (<= 12 words), in Vietnamese, and emotionally natural.
                                                         - The text should feel like a brief inner thought that matches the scene (e.g., “ráng thêm chút nữa thôi”, “cố lên, gần xong rồi”, “hít sâu nào”, “được đó, tiến triển tốt”).
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
