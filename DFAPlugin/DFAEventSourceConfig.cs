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
            public Offsets Offset { get; set; } = new Offsets();

            public class Offsets
            {
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
            /**
             * Now Renameing Key "DFA" to "qitana.DFA" started v2.1.1
             * In future release,  Load "DFA" section will remove.
             */

            var result = new DFAEventSourceConfig();
            JObject obj = null;

            if (pluginConfig.EventSourceConfigs.ContainsKey("qitana.DFA"))
            {
                obj = pluginConfig.EventSourceConfigs["qitana.DFA"];
            } 
            else if (pluginConfig.EventSourceConfigs.ContainsKey("DFA"))
            {
                obj = pluginConfig.EventSourceConfigs["DFA"];
            }

            if (obj != null)
            {
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
            }

            return result;
        }

        public void SaveConfig(IPluginConfig pluginConfig)
        {
            /**
             * Now Renameing Key "DFA" to "qitana.DFA" started v2.1.1
             * In future release,  Save "DFA" => null section will remove.
             */
            if (pluginConfig.EventSourceConfigs.ContainsKey("DFA"))
            {
                pluginConfig.EventSourceConfigs.Remove("DFA");
            }

            pluginConfig.EventSourceConfigs["qitana.DFA"] = JObject.FromObject(this);
        }
    }
}
