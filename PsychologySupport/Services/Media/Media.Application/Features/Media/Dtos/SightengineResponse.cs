namespace Media.Application.Features.Media.Dtos
{
    public record SightengineResponse(
        string Status,
        Nudity Nudity,
        Weapon Weapon,
        Alcohol? Alcohol,
        Drugs? Drugs,
        Violence? Violence,
        MediaInfo? Media
    );

    public record Nudity(
        double None,                  // = Safe
        double SexualActivity,
        double SexualDisplay,
        double Erotica,
        double Suggestive
    );

    public record Weapon(WeaponClasses Classes);

    public record WeaponClasses(
        double Firearm,
        double Knife
    );

    public record Alcohol(double Prob);

    public record Drugs(double Prob);

    public record Violence(
        double Prob,
        ViolenceClasses Classes
    );

    public record ViolenceClasses(
        double PhysicalViolence
    );

    public record MediaInfo(
        string Id,
        string Uri
    );
}
