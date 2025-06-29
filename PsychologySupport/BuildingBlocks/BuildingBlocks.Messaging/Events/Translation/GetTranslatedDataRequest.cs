namespace BuildingBlocks.Messaging.Events.Translation;

public record GetTranslatedDataRequest(Dictionary<string, string> Originals, string TargetLanguage);