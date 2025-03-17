namespace Scheduling.API.Dtos
{
    public class TotalSessionDto
    {
        public Guid SessionId { get; set; }
        public DateOnly Order { get; set; }
        public float Percentage { get; set; }

    }
}
