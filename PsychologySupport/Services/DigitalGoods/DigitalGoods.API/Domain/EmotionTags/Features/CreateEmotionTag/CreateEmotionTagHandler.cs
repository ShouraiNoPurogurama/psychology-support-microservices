using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.DigitalGood;
using DigitalGoods.API.Data;
using DigitalGoods.API.Enums;
using DigitalGoods.API.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace DigitalGoods.API.Domain.EmotionTags.Features.CreateEmotionTag;

// Request/Response
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

// Command
public record CreateEmotionTagCommand(
    Guid IdempotencyKey,
    string Code,
    string DisplayName,
    string? UnicodeCodepoint,
    string? Topic,
    int SortOrder,
    bool IsActive,
    EmotionTagScope Scope,
    Guid? MediaId
) : IdempotentCommand<CreateEmotionTagResult>(IdempotencyKey);

public record CreateEmotionTagResult(
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

// Handler
public class CreateEmotionTagHandler
    : ICommandHandler<CreateEmotionTagCommand, CreateEmotionTagResult>
{
    private readonly DigitalGoodsDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateEmotionTagHandler(DigitalGoodsDbContext dbContext, IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateEmotionTagResult> Handle(CreateEmotionTagCommand request, CancellationToken cancellationToken)
    {
        // check code duplicate
        var exists = await _dbContext.EmotionTags
            .AnyAsync(x => x.Code == request.Code, cancellationToken);

        if (exists)
            throw new ConflictException($"EmotionTag with code {request.Code} already exists.");

      
        var tag = EmotionTag.Create(
            request.Code,
            request.DisplayName,
            request.UnicodeCodepoint,
            request.Topic,
            request.SortOrder,
            request.IsActive,
            request.Scope,
            request.MediaId,
            createdBy: "admin" 
        );

        _dbContext.EmotionTags.Add(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // raise integration event
        var integrationEvent = new EmotionTagCreatedIntegrationEvent(
            tag.Id,
            tag.Code,
            tag.DisplayName,
            tag.Scope.ToString(),
            tag.MediaId
        );

        await _publishEndpoint.Publish(integrationEvent, cancellationToken);

        return new CreateEmotionTagResult(
            tag.Id,
            tag.Code,
            tag.DisplayName,
            tag.UnicodeCodepoint,
            tag.Topic,
            tag.SortOrder,
            tag.IsActive,
            tag.Scope,
            tag.MediaId
        );
    }
}

