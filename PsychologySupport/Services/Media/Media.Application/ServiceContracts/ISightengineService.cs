using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Application.ServiceContracts
{
    public interface ISightengineService
    {
        Task<(bool IsSafe, List<string> Violations)> CheckImageAsync(IFormFile file);
        Task<(bool IsSafe, List<string> Violations)> CheckImageWithWorkflowAsync(IFormFile file);
    }
}
