using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Wellness.Domain.Aggregates.ModuleSections.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ModuleCategory
    {
        OvercomeAnxiety,      // Giảm lo âu, sợ hãi
        ManageDepression,     // Vượt qua buồn bã, trầm cảm
        BuildSelfWorth,       // Tăng tự tin, động lực
        ManageStress,         // Quản lý căng thẳng, quá tải
        ImproveSleep,         // Cải thiện giấc ngủ
        HealSelfHarm,         // Hỗ trợ tự hại, ý nghĩ tiêu cực
        BoostRelationships,   // Cải thiện giao tiếp, gắn kết
        PracticeMindfulness   // Rèn luyện chánh niệm, nhận thức
    }
}
