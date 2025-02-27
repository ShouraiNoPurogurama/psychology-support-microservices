using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Application.Dtos
{
    public record TestResultDto(Guid Id, Guid TestId, int PatientId, DateTime TakenAt, string SeverityLevel);
}
