using BuildingBlocks.Exceptions;
using Carter;
using Mapster;
using Media.Application.Features.Media.Commands.GenerateSticker;
using Media.Application.Features.Media.Dtos;
using Media.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Media.API.Endpoints;

public record GenerateStickerRequest(Guid RewardId, string StickerGenerationPrompt);

public record GenerateStickerResponse(
    Guid MediaId,
    MediaState State,
    MediaVariantDto Original
);

public class GenerateStickerEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/media/sticker",
                async (GenerateStickerRequest request,
                    ILogger<GenerateStickerEndpoint> logger, ISender sender) =>
                {
                    var command = new GenerateStickerCommand(request.RewardId, request.StickerGenerationPrompt);

                    var result = await sender.Send(command);

                    var response = result.Adapt<GenerateStickerResponse>();
                    return Results.Ok(response);
                })
            .WithName("GenerateSticker")
            .WithTags("Media")
            .Produces<GenerateStickerResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithDescription("GenerateSticker")
            .WithSummary("GenerateSticker");
    }
}