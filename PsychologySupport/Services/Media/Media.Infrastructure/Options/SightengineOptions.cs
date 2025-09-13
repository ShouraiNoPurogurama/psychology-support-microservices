using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Infrastructure.Options
{
    public class SightengineOptions
    {
        public string ApiUser { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string WorkflowId { get; set; } = string.Empty;
    }
}
