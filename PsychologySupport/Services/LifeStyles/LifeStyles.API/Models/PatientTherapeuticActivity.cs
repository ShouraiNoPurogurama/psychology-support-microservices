using BuildingBlocks.Enums;

namespace LifeStyles.API.Models;

public class PatientTherapeuticActivity
{
    public Guid PatientProfileId { get; set; }
    public Guid TherapeuticActivityId { get; set; }
    public PreferenceLevel PreferenceLevel { get; set; }
}