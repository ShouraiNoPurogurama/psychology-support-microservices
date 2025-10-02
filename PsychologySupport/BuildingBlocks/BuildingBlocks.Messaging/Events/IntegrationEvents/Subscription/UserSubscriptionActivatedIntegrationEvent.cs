using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Subscription
{
    public record UserSubscriptionActivatedIntegrationEvent(Guid SubjectRef, string PlanName) : IntegrationEvent;
}
