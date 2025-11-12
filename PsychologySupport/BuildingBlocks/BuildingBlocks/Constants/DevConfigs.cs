namespace BuildingBlocks.Constants;

public class DevConfigs
{
    private static readonly string[] DevSubjectRefs =
    [
        "765caad0-89ab-44a0-8471-86f76d3a5b04"
    ];

    public static bool IsDeveloperAccount(Guid subjectRef)
    {
        return DevSubjectRefs.Contains(subjectRef.ToString());
    }
}