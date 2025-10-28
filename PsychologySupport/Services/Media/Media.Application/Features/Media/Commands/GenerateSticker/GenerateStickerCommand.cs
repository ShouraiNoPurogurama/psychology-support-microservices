using BuildingBlocks.CQRS;
using Media.Application.Features.Media.Dtos;
using Media.Domain.Enums;

namespace Media.Application.Features.Media.Commands.GenerateSticker;

public record GenerateStickerCommand(Guid RewardId, string StickerGenerationPrompt) : ICommand<GenerateStickerResult>;

public record GenerateStickerResult(
    Guid MediaId,
    MediaState State,
    MediaVariantDto Original
);