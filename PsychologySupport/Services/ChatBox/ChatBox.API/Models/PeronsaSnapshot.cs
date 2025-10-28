using Newtonsoft.Json;

namespace ChatBox.API.Models;

public class PersonaSnapshot
{
    // public string FullName { get; set; } = "";
    public string Gender { get; set; } = "";
    public string BirthDate { get; set; } = "";
    public string JobTitle { get; set; } = "";
    
    public int Age
    {
        get
        {
            try
            {
                // Loại bỏ dấu " nếu JSON chứa trong string
                var cleaned = BirthDate.Trim('"');
                var birth = DateTimeOffset.Parse(cleaned, null, System.Globalization.DateTimeStyles.AssumeUniversal);
                var now = DateTimeOffset.UtcNow;
                var age = now.Year - birth.Year;
                if (now < birth.AddYears(age)) age--; // chưa tới sinh nhật năm nay
                return age;
            }
            catch
            {
                return 25; // fallback default
            }
        }
    }
    // public string EducationLevel { get; set; } = "";
    // public string IndustryName { get; set; } = "";
    // public string PersonalityTraits { get; set; } = "";
    // public string Allergies { get; set; } = "";

    public static PersonaSnapshot FromJson(string json)
        => JsonConvert.DeserializeObject<PersonaSnapshot>(json)
           ?? throw new InvalidOperationException("Không thể parse PersonaSnapshot từ JSON");

    public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
}
