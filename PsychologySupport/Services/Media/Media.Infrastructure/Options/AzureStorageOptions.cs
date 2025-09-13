using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Infrastructure.Options
{
    public class AzureStorageOptions
    {
        public string ConnectionString { get; set; } = null!;
        public string ContainerName { get; set; } = null!;
        public string CdnBaseUrl { get; set; } = string.Empty;
    }
}
