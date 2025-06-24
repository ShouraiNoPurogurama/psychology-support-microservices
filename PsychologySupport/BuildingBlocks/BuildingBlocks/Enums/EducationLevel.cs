using System.Text.Json.Serialization;

namespace BuildingBlocks.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum EducationLevel
    {
        Elementary,
        MiddleSchool,
        Student,
        HighSchool,
        College,
        Bachelor,
        Master,
        Doctorate,
        PhD,
        Other
    }

    public static class EducationLevelExtensions
    {
        public static string ToReadableString(this EducationLevel educationLevel)
        {
            return educationLevel switch
            {
                EducationLevel.Elementary => "Elementary School",
                EducationLevel.MiddleSchool => "Middle School",
                EducationLevel.HighSchool => "High School",
                EducationLevel.College => "College/University",
                EducationLevel.Bachelor => "Bachelor's Degree",
                EducationLevel.Master => "Master's Degree",
                EducationLevel.Doctorate => "Doctorate Degree",
                EducationLevel.Other => "Other",
                _ => educationLevel.ToString()
            };
        }
    }
}
