using BuildingBlocks.Messaging.Events.ChatBox;
using ChatBox.API.Data;
using ChatBox.API.Models;
using MassTransit;

namespace ChatBox.API.Domains.Chatboxes.EventHandlers;
 
public class BookingCreatedIntegrationEventHandler(ChatBoxDbContext dbContext) : IConsumer<BookingCreatedIntegrationEvent>
{
    public async Task Consume(ConsumeContext<BookingCreatedIntegrationEvent> context)
    {
        var eventData = context.Message;
        var doctorPatient = new DoctorPatientBooking
        {
            DoctorUserId = eventData.DoctorUserId,
            PatientUserId = eventData.PatientUserId,
            BookingId = eventData.BookingId
        };
        
        dbContext.DoctorPatients.Add(doctorPatient);
        await dbContext.SaveChangesAsync();
    }
}