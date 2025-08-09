namespace BuildingBlocks.Messaging.Events.Translation;

public record GetTranslatedDataResponse(Dictionary<string, string> Translations);