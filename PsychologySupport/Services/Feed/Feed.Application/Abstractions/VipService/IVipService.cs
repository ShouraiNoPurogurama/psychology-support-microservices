using System;
using System.Threading;
using System.Threading.Tasks;

namespace Feed.Application.Abstractions.VipService;

public interface IVipService
{
    Task<bool> IsVipAsync(Guid aliasId, CancellationToken ct);
    Task UpdateVipStatusAsync(Guid aliasId, bool isVip, CancellationToken ct);
}
