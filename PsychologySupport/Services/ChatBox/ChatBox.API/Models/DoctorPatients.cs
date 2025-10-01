namespace ChatBox.API.Models;

public class DoctorPatientBooking
{
    public Guid BookingId { get; set; }
    public Guid DoctorUserId { get; set; }
    public Guid PatientUserId { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}