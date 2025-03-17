using Scheduling.API.Enums;
using Scheduling.API.Models;

namespace Scheduling.API.Dtos;

public record ScheduleActivitiesSpecificationDto(ScheduleActivity ScheduleActivity ,Guid SpecificActivityId);
