using BuildingBlocks.DDD;
using DigitalGoods.API.Enums;
using System;
using System.Collections.Generic;

namespace DigitalGoods.API.Models;

public partial class Inventory : Entity<Guid>
{
    public Guid Subject_ref { get; set; }

    public Guid DigitalGoodId { get; set; }

    public int Quantity { get; set; }

    public InventoryStatus Status { get; set; }

    public DateTimeOffset GrantedAt { get; set; }

    public DateTimeOffset? ExpiredAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public Guid? LastModifiedBy { get; set; }

    public virtual DigitalGood DigitalGood { get; set; } = null!;

    private Inventory() { }

    public static Inventory Create(Guid subjectRef, Guid digitalGoodId, int quantity,
        DateTimeOffset grantedAt, DateTimeOffset expiredAt)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive.", nameof(quantity));

        var now = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7));
        return new Inventory
        {
            Id = Guid.NewGuid(),
            Subject_ref = subjectRef,
            DigitalGoodId = digitalGoodId,
            Quantity = quantity,
            Status = InventoryStatus.Active,
            GrantedAt = grantedAt,
            ExpiredAt = expiredAt,
            CreatedAt = now,
            CreatedBy = subjectRef,
            LastModified = now,
            LastModifiedBy = subjectRef
        };
    }

    public void UpdateQuantity(int newQuantity, Guid modifiedBy)
    {
        if (newQuantity < 0) throw new ArgumentException("Quantity cannot be negative.", nameof(newQuantity));
        Quantity = newQuantity;
        LastModified = DateTimeOffset.UtcNow;
        LastModifiedBy = modifiedBy;
    }

}
