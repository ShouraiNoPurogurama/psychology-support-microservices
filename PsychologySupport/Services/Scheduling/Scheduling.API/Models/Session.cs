using MassTransit.SagaStateMachine;

namespace Scheduling.API.Models
{
    public class Session
    {
        public Guid Id { get; set; }
        public Guid ScheduleId { get; set; }
        public string Purpose { get; set; }
        public int Order { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<ScheduleActivity> Activities { get; set; } = new List<ScheduleActivity>();
    }
}
