using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wallet.Infrastructure.Resilience.Rules
{
    public sealed class IdempotencyCacheOptions
    {
        public TimeSpan EntryTtl { get; init; } = TimeSpan.FromMinutes(2);
        public TimeSpan LockTtl { get; init; } = TimeSpan.FromSeconds(30);
        public int MaxResponseBytes { get; init; } = 256 * 1024; // 256KB
    }
}
