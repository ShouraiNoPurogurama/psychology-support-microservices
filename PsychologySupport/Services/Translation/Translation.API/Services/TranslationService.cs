using BuildingBlocks.Enums;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Translation.API.Data;
using Translation.API.Protos;

namespace Translation.API.Services;

public class TranslationService : Protos.TranslationService.TranslationServiceBase
{
    private readonly ILogger<TranslationService> _logger;
    private readonly TranslationDbContext _db;
    private readonly GeminiService _translationClient;

    public TranslationService(
        ILogger<TranslationService> logger,
        TranslationDbContext db,
        GeminiService translationClient)
    {
        _logger = logger;
        _db = db;
        _translationClient = translationClient;
    }

    public override async Task<TranslateDataResponse> TranslateData(TranslateDataRequest request, ServerCallContext context)
    {
        var originals = request.Originals.ToDictionary(k => k.Key, k => k.Value);
        var targetLanguage = Enum.Parse<SupportedLang>(request.TargetLanguage);

        var requestedKeys = originals.Keys.Distinct().ToList();

        // 1. Lấy các bản dịch đã có
        var existing = await _db.Translations
            .Where(t => requestedKeys.Contains(t.TextKey) && t.Lang == targetLanguage)
            .ToListAsync(context.CancellationToken);

        var existingDict = existing.ToDictionary(t => t.TextKey, t => t.TranslatedValue);

        // 2. Lọc ra các keys chưa có bản dịch
        var missingKeys = requestedKeys.Except(existingDict.Keys).ToList();

        // 3. Kiểm tra bản gốc "en" đã có chưa
        var enExistingKeys = await _db.Translations
            .Where(t => missingKeys.Contains(t.TextKey) && t.Lang == SupportedLang.en)
            .Select(t => t.TextKey)
            .ToListAsync(context.CancellationToken);

        var enMissingKeys = missingKeys.Except(enExistingKeys).ToList();

        // 4. Insert bản gốc "en" nếu chưa có
        if (enMissingKeys.Any())
        {
            var enEntries = enMissingKeys.Select(key => new Models.Translation
            {
                Id = Guid.NewGuid(),
                TextKey = key,
                Lang = SupportedLang.en,
                TranslatedValue = originals[key],
                CreatedAt = DateTimeOffset.UtcNow,
                IsStable = true
            });

            await _db.Translations.AddRangeAsync(enEntries, context.CancellationToken);
            await _db.SaveChangesAsync(context.CancellationToken);
        }

        // 5. Lấy lại bản gốc "en" để dịch
        var originalEnglishDict = requestedKeys
            .Where(k => missingKeys.Contains(k))
            .ToDictionary(k => k, k => originals[k]);

        _logger.LogInformation("[DEBUG 1] Translating {Count} keys from English to {TargetLanguage}", originalEnglishDict.Count, targetLanguage);

        // 6. Dịch bằng Gemini
        var translatedFromGemini = await _translationClient.TranslateKeysAsync(originalEnglishDict);

        // Replace the problematic line with the following:
        var viEntries = translatedFromGemini.Select(kv => new Models.Translation
        {
            Id = Guid.NewGuid(),
            TextKey = kv.Key,
            Lang = targetLanguage,
            TranslatedValue = kv.Value,
            CreatedAt = DateTimeOffset.UtcNow,
            IsStable = false
        });

        await _db.Translations.AddRangeAsync(viEntries, context.CancellationToken);
        await _db.SaveChangesAsync(context.CancellationToken);

        // 8. Gộp bản dịch cũ + mới
        var result = existingDict
            .Concat(translatedFromGemini)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        return new TranslateDataResponse { Translations = { result } };
    }
}