using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BuildingBlocks.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Emotion
    {
        Happy,
        Sad,
        Angry,
        Anxious,      
        Stressed,     
        Hopeless,      
        Empty,         
        Guilty,       
        Ashamed,       
        Irritable,     
        Tired,         
        Lonely,        
        Fearful,           
        Helpless       
    }
}
