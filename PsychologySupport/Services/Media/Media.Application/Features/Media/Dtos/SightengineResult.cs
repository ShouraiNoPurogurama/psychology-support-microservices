namespace Media.Application.Features.Media.Dtos
{
    public record SightengineResult(
       bool IsSafe,
       string RawJson,
       string? WorkflowId,
       double? Score
   );
}
