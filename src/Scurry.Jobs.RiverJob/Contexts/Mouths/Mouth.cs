using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Contexts.Mouths
{
    [Serializable]
    [JsonObject]
    public class Mouth
    {
        public string Url { get; set; }

        public string Index { get; set; }

        public string Type { get; set; }

        public int MaxBulkSize { get; set; }

        public Mapping Mapping { get; set; }

        public bool DeleteExisting { get; set; }

        Mouth()
        {
            MaxBulkSize = 100;
            DeleteExisting = false;
        }
    }   

    [Serializable]
    [JsonObject]
    public class Mapping
    {
        public Parent Parent { get; set; }
    }

    [Serializable]
    [JsonObject]
    public class Parent
    {
        public string Type { get; set; }
    }
}
