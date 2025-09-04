namespace BuildingBlocks.Messaging.Events.Queries.Translation;

public record GetTranslatedDataResponse(Dictionary<string, string> Translations);