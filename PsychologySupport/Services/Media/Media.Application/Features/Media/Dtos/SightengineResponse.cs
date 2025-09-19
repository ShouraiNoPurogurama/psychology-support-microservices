namespace Media.Application.Features.Media.Dtos
{
    public record SightengineResponse(
    string Status,
    Nudity Nudity,
    Weapon Weapon,
    Alcohol Alcohol,
    Drugs Drugs
);

    public record Nudity(
        double Safe,
        double SexualActivity,
        double SexualDisplay
    );

    public record Weapon(double Prob);

    public record Alcohol(double Prob);

    public record Drugs(double Prob);

}
