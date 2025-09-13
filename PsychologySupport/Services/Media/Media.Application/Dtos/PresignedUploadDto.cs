using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Media.Application.Dtos
{
    public record PresignedUploadDto
    {
        public string UploadUrl { get; init; } = default!;
        public DateTime ExpiresAt { get; init; }
    }
}