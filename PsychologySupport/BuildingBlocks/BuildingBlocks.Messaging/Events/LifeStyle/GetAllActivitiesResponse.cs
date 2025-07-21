using BuildingBlocks.Messaging.Dtos.LifeStyles;

namespace BuildingBlocks.Messaging.Events.LifeStyle;

public record GetAllActivitiesResponse(List<ActivityDto> Activities);
