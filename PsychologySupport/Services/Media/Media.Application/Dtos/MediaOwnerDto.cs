using Media.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Application.Dtos
{
    public record MediaOwnerDto
    {
        public MediaOwnerType MediaOwnerType { get; init; }
        public Guid MediaOwnerId { get; init; }
    }
}
