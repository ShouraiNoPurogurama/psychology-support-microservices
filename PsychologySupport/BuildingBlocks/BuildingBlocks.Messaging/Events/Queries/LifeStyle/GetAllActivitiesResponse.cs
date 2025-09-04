using BuildingBlocks.Messaging.Dtos.LifeStyles;

namespace BuildingBlocks.Messaging.Events.Queries.LifeStyle;

public record GetAllActivitiesResponse(List<ActivityDto> Activities);
