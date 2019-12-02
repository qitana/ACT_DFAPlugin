using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RainbowMage.OverlayPlugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Qitana.DFAPlugin
{
    [Serializable]
    public class DFAEventSourceConfig
    {
        public string StructuresURL { get; set; } = "https://qitana.github.io/ACT_DFAPlugin/data/structures.json";
        public string TextToSpeech { get; set; } = "${matched}";
        public List<Structure> Structures { get; set; } = new List<Structure>();
        
        public class Structure
        {
            public string Name { get; set; }
            public ushort Opcode { get; set; }
            public ushort Opcode2 { get; set; }
            public Offsets Offset { get; set; } = new Offsets();

            public class Offsets
            {
                public int Opcode2 { get; set; }
                public int RouletteCode { get; set; }
                public int DungeonCode { get; set; }
                public int WaitList { get; set; }
                public int WaitTime { get; set; }
                public int Tank { get; set; }
                public int TankMax { get; set; }
                public int Healer { get; set; }
                public int HealerMax { get; set; }
                public int Dps { get; set; }
                public int DpsMax { get; set; }
            }
        }

        public static DFAEventSourceConfig LoadConfig(IPluginConfig pluginConfig)
        {
            var result = new DFAEventSourceConfig();

            if (pluginConfig.EventSourceConfigs.ContainsKey("DFA"))
            {
                var obj = pluginConfig.EventSourceConfigs["DFA"];

                if (obj.TryGetValue("StructuresURL", out JToken structuresURL))
                {
                    result.StructuresURL = structuresURL.ToString();
                }

                if (obj.TryGetValue("TextToSpeech", out JToken textToSpeech))
                {
                    result.TextToSpeech = textToSpeech.ToString();
                }

                if (obj.TryGetValue("Structures", out JToken structures))
                {
                    result.Structures = structures.ToObject<List<Structure>>();
                }

                /*
                if (obj.TryGetValue("data", out JToken value))
                {
                    result.data = value.ToObject<Dictionary<string, string>>();
                }

                if (obj.TryGetValue("stringdata", out value))
                {
                    result.stringdata = value.ToString();
                }

                if (obj.TryGetValue("booldata", out value))
                {
                    result.booldata = value.ToObject<bool>();
                }
                */
            }

            return result;
        }

        public void SaveConfig(IPluginConfig pluginConfig)
        {
            pluginConfig.EventSourceConfigs["DFA"] = JObject.FromObject(this);
        }

        
    }
}
