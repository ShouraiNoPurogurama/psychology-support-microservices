﻿namespace BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;

public record UpgradeSubscriptionPaymentSuccessIntegrationEvent(Guid SubjectRef,Guid SubscriptionId) : IntegrationEvent;