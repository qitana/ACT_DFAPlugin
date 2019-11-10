using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qitana.DFAPlugin
{
    public class DFADataModel
    {
        public string version { get; set; }
        public Dictionary<string, instance> instances { get; set; }
        public Dictionary<string, string> roulettes { get; set; }

        public class instance
        {
            public string name { get; set; }
            public int tank { get; set; }
            public string healer { get; set; }
            public string dps { get; set; }
        }
        public class Names
        {
            public string ja_jp { get; set; }
            public string en_us { get; set; }
            public string de_de { get; set; }
            public string fr_fr { get; set; }
            public string ko_kr { get; set; }
        }

    }
}
