using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Media.Application.Features.Media.Dtos;

namespace Media.Application.ServiceContracts
{
    public interface ISightengineService
    {
        Task<SightengineResult> CheckImageWithWorkflowAsync(IFormFile file);
    }
}
