﻿namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;

public record UpgradeSubscriptionPaymentSuccessIntegrationEvent(Guid SubscriptionId) : IntegrationEvent;