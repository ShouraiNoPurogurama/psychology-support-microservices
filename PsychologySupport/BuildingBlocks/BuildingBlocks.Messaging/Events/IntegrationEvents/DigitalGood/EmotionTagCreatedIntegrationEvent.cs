using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.IntegrationEvents.DigitalGood
{
    public record EmotionTagCreatedIntegrationEvent(
        Guid EmotionTagId,
        string Code,
        string DisplayName,
        string Scope
    ) : IntegrationEvent;
}
