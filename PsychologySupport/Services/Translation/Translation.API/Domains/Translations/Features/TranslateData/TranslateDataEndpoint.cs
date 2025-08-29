using BuildingBlocks.Enums;
using Carter;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Translation.API.Domains.Translations.Features.TranslateData;

public record TranslateDataRequest(Dictionary<string, string> Originals, SupportedLang TargetLanguage = SupportedLang.vi);

public record TranslateDataResponse(Dictionary<string, string> Translations);

public class TranslateDataEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/translate", async (
                [FromBody] TranslateDataRequest request,
                ISender sender) =>
            {
                var result = await sender.Send(new TranslateDataCommand(request.Originals, request.TargetLanguage));
                return Results.Ok(new TranslateDataResponse(result.Translations));
            })
            .WithName("TranslateData")
            .WithTags("Translation")
            .Produces<TranslateDataResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Translate key-value strings via Gemini")
            .WithDescription("Auto-inserts English originals if missing, then translates to Vietnamese using Gemini.");
    }
}