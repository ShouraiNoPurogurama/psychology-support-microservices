using System.Text.Json;
using System.Text.Json.Serialization; // map tên field khi deserialize
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
/// - Bubble text được giao cho image agent tự tạo trong lúc render (không gọi text lần 2)
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
                                       - Preserve the character’s identity, silhouette, proportions, face geometry, costume cues, and color palette from the image.
                                       - The attached character is the only protagonist. Do not replace, morph, or transform them into cats or other animals.
                                       - Keep camera framing centered on this subject.
                                       """;

    // ===== (C) STYLE =====
    private const string PromptStyle = """
                                       **STYLE (transfer-only):** Masterpiece 3D animation inspired by Pixar/DreamWorks; cinematic composition; hyper-realistic soft textures (silicone/plastic glow); dramatic volumetric lighting; shallow depth of field; 8K clarity.
                                       Do not alter the subject’s identity; apply style only.
                                       """;

    // ===== (D) NEGATIVE =====
    private const string NegativePrompt = """
                                          **NEGATIVE:**
                                          - No cats/dogs/animals or new characters unless provided.
                                          - No species morphing, face swap, or subject replacement.
                                          - Avoid cluttered backgrounds, heavy text overlays, watermark, or logos.
                                          """;

    // ===== (E) Filler Template (text LLM sinh ra 1 lần) =====
    private const string FillerGenerationPromptTemplate = """
                                                          Generate the **FILLER** (ACTION/SETTING/DETAILS) for the same subject from the attached image. 
                                                          Do not invent a new species/animal; keep the attached subject as-is.

                                                          **Data:**
                                                          "age": {0},
                                                          "gender": "{1}",
                                                          "job": "{2}",
                                                          "chat_summary_context": "Dựa trên bản tóm tắt cuộc trò chuyện này, suy ra cảm xúc chính xác: '{3}'. Dùng cảm xúc này cho [emotion]."

                                                          **FILLER Formula (about the attached subject):**
                                                          "The subject (same as in the attached image) embodies a [age]-year-old [gender] [job], currently [emotion].
                                                          ACTION/SETTING: The subject is [AUTO-GENERATE action personalized to 'job' and 'chat_summary_context'], using relevant props. Expression shows [AUTO-GENERATE facial expression matching 'emotion']; eyes are [AUTO-GENERATE eye description].
                                                          SCENE: Set in [AUTO-GENERATE location tied to 'job'] with [AUTO-GENERATE light/atmosphere supporting 'emotion'].
                                                          DETAILS: Surroundings include [AUTO-GENERATE 3–5 small signature objects specific to 'job' and 'emotion'].
                                                          LIGHTING: [AUTO-GENERATE dramatic lighting effect], highlighting the subject’s warm glow against [AUTO-GENERATE background color/space]."
                                                          """;

    // ===== (F) Bubble Instruction (image agent tự nghĩ & vẽ) =====
    private const string BubbleAutoInstructionTemplate = """
                                                         **SPEECH BUBBLE (overlay, auto-content):**
                                                         - Add a small, **soft-edged, bubbly/elliptical** speech bubble near the subject’s head (avoid long, rigid rectangular shapes).
                                                         - Style: semi-transparent white fill, very soft shadow, no harsh outlines.
                                                         - Do not block the subject’s face or key props; keep it minimal.
                                                         - **Auto-compose one *single, concise* friendly phrase (ideally 4-10 words in English)**.
                                                         - The phrase must be **highly specific** to the user's chat context/emotion and the scene's action.
                                                         - **AVOID generic encouragement** like "Keep up the good work" or "Hope your day is great."
                                                         - Tone: warm, specific, and natural; no emojis or quotes.
                                                         """;

    public async Task<RewardGenerationDataDto> PrepareGenerationDataAsync(Guid aliasId, Guid rewardId, Guid chatSessionId,
        CancellationToken ct)
    {
        logger.LogInformation("Generating prompt (single roundtrip) for RewardId: {RewardId}, AliasId: {AliasId}", rewardId,
            aliasId);

        // 1) Lấy dữ liệu từ Chatbox gRPC
        var chatData = await GetChatDataAsync(aliasId, chatSessionId, ct);

        // 2) ===== LOGIC SINH FILLER ĐÃ CẬP NHẬT =====
        // Ưu tiên dùng 'ImageContext' mới để tạo ảnh
        var promptContext = chatData.Summary.Metadata?.ImageContext?.Trim();

        // Fallback 1: Nếu 'ImageContext' rỗng, dùng 'EmotionContext' cũ
        if (string.IsNullOrWhiteSpace(promptContext))
        {
            promptContext = chatData.Summary.Metadata?.EmotionContext?.Trim();
        }

        // Fallback 2: Nếu cả hai đều rỗng, dùng "neutral"
        if (string.IsNullOrWhiteSpace(promptContext))
        {
            promptContext = "neutral";
        }

        var fillerPrompt = string.Format(
            FillerGenerationPromptTemplate,
            TimeUtils.GetAgeFromDateTimeOffsetStr(chatData.Persona.BirthDate),
            chatData.Persona.Gender,
            chatData.Persona.JobTitle,
            promptContext
        );

        logger.LogDebug("Calling Gemini (text) to generate FILLER for RewardId: {RewardId}", rewardId);
        var generatedFiller = await geminiService.GenerateTextAsync(fillerPrompt, ct);

        // 3) Ghép final prompt (Giữ nguyên)
        var finalPrompt =
            $@"{SubjectLock}
{PromptStyle}
{NegativePrompt}

{PromptBase}

{generatedFiller}

{BubbleAutoInstructionTemplate}";

        // Để trace: lưu lại filler; bubble là auto nên không có text cụ thể
        var storedFiller = $"{generatedFiller}\n\n[BUBBLE_TEXT]\n(Auto by image agent)";

        return new RewardGenerationDataDto(
            UserId: chatData.UserId,
            FinalPrompt: finalPrompt,
            PromptBase: PromptBase,
            PromptFiller: storedFiller
        );
    }

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
            {
                logger.LogWarning("Chat data for AliasId {AliasId} is missing persona or summary.", aliasId);
                throw new InvalidOperationException("Missing required chat data to generate prompt.");
            }

            var persona = JsonSerializer.Deserialize<PersonaSnapshot>(response.PersonaSnapshotJson)
                          ?? new PersonaSnapshot();
            
            var userId = Guid.Parse(response.UserId);

            // Tách chuỗi JSON bằng "---" và lấy cái CUỐI CÙNG (mới nhất)
            var jsonSummaries = response.SummarizationJson.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);
            var latestSummaryJson = jsonSummaries.LastOrDefault()?.Trim();

            ChatSummaryJson summaryJson;
            if (!string.IsNullOrWhiteSpace(latestSummaryJson))
            {
                try
                {
                    summaryJson = JsonSerializer.Deserialize<ChatSummaryJson>(latestSummaryJson)
                                  ?? new ChatSummaryJson();
                }
                catch (JsonException jsonEx)
                {
                    logger.LogError(jsonEx, "Failed to parse LATEST summary JSON for AliasId: {AliasId}. Raw JSON: {Json}",
                        aliasId, latestSummaryJson);
                    throw new InvalidOperationException("Failed to parse latest summary JSON.", jsonEx);
                }
            }
            else
            {
                logger.LogWarning(
                    "SummarizationJson for AliasId {AliasId} was not empty but contained no valid '---' separated blocks.",
                    aliasId);
                throw new InvalidOperationException("SummarizationJson format error.");
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

    // ===== DTO nội bộ để deserialize JSON từ gRPC =====
    private class PersonaSnapshot
    {
        public string Gender { get; set; } = "";
        public string BirthDate { get; set; } = "";
        public string JobTitle { get; set; } = "";
    }

    // (DTO ChatSummaryJson giữ nguyên)
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

    // ===== DTO NÀY ĐÃ ĐƯỢC CẬP NHẬT =====
    private class ChatMetadata
    {
        [JsonPropertyName("emotionContext")]
        public string? EmotionContext { get; set; }

        [JsonPropertyName("topic")]
        public string? Topic { get; set; }

        // ===== TRƯỜNG MỚI ĐÃ ĐƯỢC THÊM =====
        [JsonPropertyName("imageContext")]
        public string? ImageContext { get; set; }
    }

    // (Record ChatSummary giữ nguyên)
    private record ChatSummary(
        string Current,
        string Persist,
        ChatMetadata? Metadata,
        DateTimeOffset? CreatedAt)
    {
        public string FullSummary
        {
            get
            {
                var topic = string.IsNullOrWhiteSpace(Metadata?.Topic) ? "" : $" [Topic: {Metadata!.Topic}]";
                return $"{Current} {Persist}{topic}".Trim();
            }
        }
    }
}