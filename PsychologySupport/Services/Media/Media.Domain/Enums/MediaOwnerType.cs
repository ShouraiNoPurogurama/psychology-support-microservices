using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Media.Domain.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MediaOwnerType
    {
        Post,
        Comment,
        Chat,
        Profile,
        EmotionTag,
        DigitalGood,
        Audio
    }

}
