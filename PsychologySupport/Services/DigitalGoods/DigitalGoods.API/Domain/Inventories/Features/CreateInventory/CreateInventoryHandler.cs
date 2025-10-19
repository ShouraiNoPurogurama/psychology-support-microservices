using BuildingBlocks.CQRS;
using BuildingBlocks.Exceptions;
using BuildingBlocks.Messaging.Events.IntegrationEvents.DigitalGood;
using DigitalGoods.API.Data;
using DigitalGoods.API.Enums;
using DigitalGoods.API.Models;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Pii.API.Protos;

namespace DigitalGoods.API.Domain.Inventories.Features.CreateInventory;

public record CreateInventoryCommand(
    Guid SubjectRef,
    DateTimeOffset GrantedAt,
    DateTimeOffset ExpiredAt,
    Guid? EmotionTagId = null,
    Guid? GiftId = null
) : ICommand<CreateInventoryResult>;

public record CreateInventoryResult(
    string Status,
    DateTimeOffset GrantedAt
);

internal class CreateInventoryHandler : ICommandHandler<CreateInventoryCommand, CreateInventoryResult>
{
    private readonly DigitalGoodsDbContext _dbContext;
    private readonly PiiService.PiiServiceClient _piiClient;
    private readonly IPublishEndpoint _publishEndpoint;

    public CreateInventoryHandler(
        DigitalGoodsDbContext dbContext,
        PiiService.PiiServiceClient piiClient,
        IPublishEndpoint publishEndpoint)
    {
        _dbContext = dbContext;
        _piiClient = piiClient;
        _publishEndpoint = publishEndpoint;
    }

    public async Task<CreateInventoryResult> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
    {
        // Tạo inventory cho tất cả DigitalGoods đang active
        var activeGoods = await _dbContext.DigitalGoods
            .Where(d => d.IsActive)
            .ToListAsync(cancellationToken);

        if (!activeGoods.Any())
            throw new NotFoundException("No active digital goods found to create inventory for.");

        foreach (var good in activeGoods)
        {
            var inventory = Inventory.Create(
                subjectRef: request.SubjectRef,
                digitalGoodId: good.Id,
                quantity: 1,
                grantedAt: request.GrantedAt,
                expiredAt: request.ExpiredAt
            );

            _dbContext.Inventories.Add(inventory);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        // SubjectRef thành AliasId bằng PiiService
        var aliasResponse = await _piiClient.ResolveAliasIdBySubjectRefAsync(
            new ResolveAliasIdBySubjectRefRequest { SubjectRef = request.SubjectRef.ToString() },
            cancellationToken: cancellationToken
        );

        var aliasId = Guid.Parse(aliasResponse.AliasId);



        var digitalGoodGrantedEvent = new UserDigitalGoodGrantedIntegrationEvent(
            AliasId: aliasId,
            ValidFrom: request.GrantedAt,
            ValidTo: request.ExpiredAt
        );

        await _publishEndpoint.Publish(digitalGoodGrantedEvent, cancellationToken);

        return new CreateInventoryResult(
            Status: InventoryStatus.Active.ToString(),
            GrantedAt: request.GrantedAt
        );
    }
}
