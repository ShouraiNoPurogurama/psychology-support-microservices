namespace Alias.API.Aliases.Models;

public class Audit
{
    public record CreatedDetails(string Source, string Label);

    public record LabelUpdatedDetails(string OldLabel, string NewLabel);
}