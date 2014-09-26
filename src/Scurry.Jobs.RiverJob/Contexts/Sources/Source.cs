using Newtonsoft.Json;
using Scurry.Jobs.RiverJob.Contexts.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Contexts.Sources
{
    [Serializable]
    [JsonObject]
    public abstract class Source
    {
        public abstract string Type { get; }

        public string Format { get; set; }
        public bool SuppressNulls { get; set; }

        protected Source()
        {
            SuppressNulls = true;
        }
    }
}
