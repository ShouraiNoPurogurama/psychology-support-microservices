using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.Subscription
{
    public record SubscriptionGetPromoAndGiftResponseEvent(
        string? PromoCode,
        Guid ? GiftId
    );
}
