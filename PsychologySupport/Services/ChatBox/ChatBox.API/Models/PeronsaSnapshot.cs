using Newtonsoft.Json;

namespace ChatBox.API.Models;

public class PersonaSnapshot
{
    public string FullName { get; set; } = "";
    public string Gender { get; set; } = "";
    public string BirthDate { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string EducationLevel { get; set; } = "";
    public string IndustryName { get; set; } = "";
    public string PersonalityTraits { get; set; } = "";
    public string Allergies { get; set; } = "";

    public static PersonaSnapshot FromJson(string json)
        => JsonConvert.DeserializeObject<PersonaSnapshot>(json)
           ?? throw new InvalidOperationException("Không thể parse PersonaSnapshot từ JSON");

    public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
}
