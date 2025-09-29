using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wellness.Application.ServiceContracts
{
    public interface IIdempotencyService
    {
        Task<Guid> CreateRequestAsync(Guid requestKey, CancellationToken cancellationToken = default);

        Task<bool> RequestExistsAsync(Guid requestKey, CancellationToken cancellationToken = default);

        Task SaveResponseAsync<T>(Guid requestKey, T response, CancellationToken cancellationToken = default);

    }
}
