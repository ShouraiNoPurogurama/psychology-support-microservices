using BuildingBlocks.CQRS;
using BuildingBlocks.Enums;
using Microsoft.EntityFrameworkCore;
using Translation.API.Data;
using Translation.API.Services;

namespace Translation.API.Features.TranslateData;

public record TranslateDataCommand(Dictionary<string, string> Originals, SupportedLang TargetLanguage) : ICommand<TranslateDataResult>;

public record TranslateDataResult(Dictionary<string, string> Translations);


public class TranslateDataHandler(
    TranslationDbContext db,
    GeminiService translationClient
) : ICommandHandler<TranslateDataCommand, TranslateDataResult>
{
    public async Task<TranslateDataResult> Handle(TranslateDataCommand request, CancellationToken cancellationToken)
    {
        var originals = request.Originals;
        var requestedKeys = originals.Keys.Distinct().ToList();

        
        // 1. Lấy các bản dịch đã có (lang = "vi")
        var existing = await db.Translations
            .Where(t => requestedKeys.Contains(t.TextKey) && t.Lang == request.TargetLanguage)
            .ToListAsync(cancellationToken);

        var existingDict = existing.ToDictionary(t => t.TextKey, t => t.TranslatedValue);

        // 2. Lọc ra các keys chưa có bản dịch
        var missingKeys = requestedKeys.Except(existingDict.Keys).ToList();

        // 3. Kiểm tra bản gốc "en" đã có chưa
        var enExistingKeys = await db.Translations
            .Where(t => missingKeys.Contains(t.TextKey) && t.Lang == SupportedLang.en)
            .Select(t => t.TextKey)
            .ToListAsync(cancellationToken);

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

            await db.Translations.AddRangeAsync(enEntries, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        // 5. Lấy lại bản gốc "en" để dịch
        var originalEnglishDict = requestedKeys
            .Where(k => missingKeys.Contains(k))
            .ToDictionary(k => k, k => originals[k]);

        // 6. Dịch bằng Gemini
        var translatedFromGemini = await translationClient.TranslateKeysAsync(originalEnglishDict);

        // 7. Lưu bản dịch mới vào DB
        var viEntries = translatedFromGemini.Select(kv => new Models.Translation
        {
            Id = Guid.NewGuid(),
            TextKey = kv.Key,
            Lang = request.TargetLanguage,
            TranslatedValue = kv.Value,
            CreatedAt = DateTimeOffset.UtcNow,
            IsStable = false
        });

        await db.Translations.AddRangeAsync(viEntries, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        // 8. Gộp bản dịch cũ + mới
        var result = existingDict
            .Concat(translatedFromGemini)
            .ToDictionary(kv => kv.Key, kv => kv.Value);

        return new TranslateDataResult(result);
    }
}
