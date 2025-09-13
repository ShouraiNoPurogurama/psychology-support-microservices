using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Application.Dtos
{
    public record SightengineResult(
       bool IsSafe,
       string RawJson,
       string? WorkflowId,
       double? Score
   );
}
