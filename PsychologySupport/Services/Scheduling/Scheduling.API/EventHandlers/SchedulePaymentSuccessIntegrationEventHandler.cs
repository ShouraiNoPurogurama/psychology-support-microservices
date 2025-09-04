using BuildingBlocks.Messaging.Events.IntegrationEvents.Payment;
using MassTransit;
using MediatR;
using Scheduling.API.Features.Schedule.CreateSchedule;

namespace Scheduling.API.EventHandlers;

public class SchedulePaymentSuccessIntegrationEventHandler(ISender sender) : IConsumer<SchedulePaymentSuccessIntegrationEvent>
{
    public async Task Consume(ConsumeContext<SchedulePaymentSuccessIntegrationEvent> context)
    {
        var command = new CreateScheduleCommand(context.Message.PatientId,null);
        
        await sender.Send(command, context.CancellationToken);
    }
}