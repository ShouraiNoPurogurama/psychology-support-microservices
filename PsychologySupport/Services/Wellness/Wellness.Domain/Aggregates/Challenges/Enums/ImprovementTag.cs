using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Wellness.Domain.Aggregates.Challenges.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ImprovementTag
    {
        MentalHealth,        // Giảm căng thẳng, tăng tập trung (tinh thần)
        PhysicalBalance,     // Tăng năng lượng, linh hoạt (thể chất)
        SocialConnection,    // Gắn kết, giảm cô lập (xã hội)
        CreativeRelaxation,  // Giải phóng cảm xúc, sáng tạo
        Combined             // Kết hợp đa chiều (tinh thần + thể chất + xã hội)
    }
}
