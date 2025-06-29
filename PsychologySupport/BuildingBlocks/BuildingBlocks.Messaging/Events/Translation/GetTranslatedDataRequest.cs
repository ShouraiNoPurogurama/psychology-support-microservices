using BuildingBlocks.Enums;

namespace BuildingBlocks.Messaging.Events.Translation;

public record GetTranslatedDataRequest(Dictionary<string, string> Originals, SupportedLang TargetLanguage);