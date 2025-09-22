using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Queries.Billing
{
    public record ValidateOrderRequest
    (
        Guid OrderId,
        string ProductCode,// PointPackageCode
        Guid SubjectRef,
        string? PromoCode,
        Guid? GiftId,
        int PointAmount,
        decimal FinalPrice
    );
}
