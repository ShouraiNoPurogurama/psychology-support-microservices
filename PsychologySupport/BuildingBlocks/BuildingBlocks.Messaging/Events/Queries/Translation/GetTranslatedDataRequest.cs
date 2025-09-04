using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Queries.Translation;

public record GetTranslatedDataRequest(Dictionary<string, string> Originals, SupportedLang TargetLanguage);