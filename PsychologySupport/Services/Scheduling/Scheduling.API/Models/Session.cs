using MassTransit.SagaStateMachine;

namespace Scheduling.API.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public string Purpose { get; set; } = string.Empty;
        public int Order { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }

        public ICollection<ScheduleActivity> Activities { get; set; } = new List<ScheduleActivity>();
    }
}
