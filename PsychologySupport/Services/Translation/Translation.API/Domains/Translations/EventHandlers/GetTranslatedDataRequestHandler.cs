using BuildingBlocks.Messaging.Events.Queries.Translation;
using MassTransit;
using MediatR;
using Translation.API.Domains.Translations.Features.TranslateData;

namespace Translation.API.Domains.Translations.EventHandlers;

public class GetTranslatedDataRequestHandler(ILogger<GetTranslatedDataRequestHandler> logger, ISender sender) : IConsumer<GetTranslatedDataRequest>
{
    public async Task Consume(ConsumeContext<GetTranslatedDataRequest> context)
    {
        logger.LogInformation("Processing GetTranslatedDataRequest for {TargetLanguage}", context.Message.TargetLanguage);
        
        var request = context.Message;
        
        var command = new TranslateDataCommand(request.Originals, request.TargetLanguage);
        
        var result = await sender.Send(command, context.CancellationToken);
        
        var response = new GetTranslatedDataResponse(result.Translations);
        
        await context.RespondAsync(response);
        
        logger.LogInformation("Translated data for {TargetLanguage} with {Count} keys", request.TargetLanguage, request.Originals.Count);
    }
}