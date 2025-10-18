using BuildingBlocks.Exceptions;
using Carter;
using DigitalGoods.API.Enums;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DigitalGoods.API.Domain.EmotionTags.Features.CreateEmotionTag;
public record CreateEmotionTagRequest(
    string Code,
    string DisplayName,
    string? UnicodeCodepoint,
    string? Topic,
    int SortOrder,
    bool IsActive,
    EmotionTagScope Scope,
    Guid? MediaId
);

public record CreateEmotionTagResponse(
    Guid EmotionTagId,
    string Code,
    string DisplayName,
    string? UnicodeCodepoint,
    string? Topic,
    int SortOrder,
    bool IsActive,
    EmotionTagScope Scope,
    Guid? MediaId
);

public class CreateEmotionTagEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/v1/emotion-tags", async (
            [FromBody] CreateEmotionTagRequest request,
            [FromHeader(Name = "Idempotency-Key")] Guid? requestKey,
            ISender sender) =>
        {
            if (requestKey is null || requestKey == Guid.Empty)
                throw new BadRequestException("Missing or invalid Idempotency-Key header.", "MISSING_IDEMPOTENCY_KEY");

            var command = new CreateEmotionTagCommand(
                IdempotencyKey: requestKey.Value,
                Code: request.Code,
                DisplayName: request.DisplayName,
                UnicodeCodepoint: request.UnicodeCodepoint,
                Topic: request.Topic,
                SortOrder: request.SortOrder,
                IsActive: request.IsActive,
                Scope: request.Scope,
                MediaId: request.MediaId
            );

            var result = await sender.Send(command);

            var response = result.Adapt<CreateEmotionTagResponse>();

            return Results.Created($"/v1/emotion-tags/{response.EmotionTagId}", response);
        })
        .WithName("CreateEmotionTag")
        .WithTags("EmotionTags")
        .Produces<CreateEmotionTagResponse>(201)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);
    }
}
