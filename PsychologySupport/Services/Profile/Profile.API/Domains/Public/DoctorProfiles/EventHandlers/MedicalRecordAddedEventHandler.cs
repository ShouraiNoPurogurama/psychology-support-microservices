using Profile.API.Domains.DoctorProfiles.Features;
using Profile.API.Domains.PatientProfiles.Events;

namespace Profile.API.Domains.DoctorProfiles.EventHandlers;

public class MedicalRecordAddedEventHandler(IBus bus, ISender sender, ILogger<MedicalRecordAddedEventHandler> logger)
    : INotificationHandler<MedicalRecordAddedEvent>
{
    public async Task Handle(MedicalRecordAddedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("DomainEvent Event handled: {DomainEvent}", notification.GetType().Name);

        var command = notification.Adapt<AddMedicalRecordCommand>();

        var result = await sender.Send(command, cancellationToken);

        if (!result.IsSuccess)
            logger.LogError("Error adding medical record to doctor id: {DoctorId}", command.MedicalRecord.DoctorProfileId);

        logger.LogInformation("Successfully added medical record to doctor id {DoctorId}", command.MedicalRecord.DoctorProfileId);
    }
}