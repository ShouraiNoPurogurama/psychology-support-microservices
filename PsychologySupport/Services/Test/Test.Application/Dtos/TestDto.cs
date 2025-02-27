using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.Application.Dtos
{
    public record TestDto(Guid Id, string CategoryName, string CategoryDescription);
}
