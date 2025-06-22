using BuildingBlocks.Enums;

namespace LifeStyles.API.Models;

public class Emotion
{
    public Guid Id { get; set; }
    
    public EmotionType Name { get; set; }
    
    public string? Description { get; set; }
    public string? IconUrl { get; set; } = null;

    public ICollection<EmotionSelection> EmotionSelections { get; set; } = new List<EmotionSelection>();
}
