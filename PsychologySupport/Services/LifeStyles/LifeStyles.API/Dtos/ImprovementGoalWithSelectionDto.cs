namespace LifeStyles.API.Dtos;

public record ImprovementGoalWithSelectionDto(
    Guid Id,
    string Name,
    string Description,
    bool IsSelected) ;