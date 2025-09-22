namespace Alias.API.Aliases.Models.Aliases;

public class Audit
{
    public record CreatedDetails(string Source, string Label);

    public record LabelUpdatedDetails(string OldLabel, string NewLabel);
}