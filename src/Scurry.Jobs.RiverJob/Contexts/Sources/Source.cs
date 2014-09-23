using Newtonsoft.Json;
using River.Components.Contexts.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace River.Components.Contexts.Sources
{
    [Serializable]
    [JsonObject]
    //[JsonConverter(typeof(SourceConverter))]
    public abstract class Source
    {
        public abstract string Type { get; }

        public bool SuppressNulls { get; set; }

        protected Source()
        {
            SuppressNulls = true;
        }
    }
}
