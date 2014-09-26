using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Contexts.Beds
{
    [Serializable]
    [JsonObject]
    public class Bed
    {
        public string Strategy { get; set; }
        public string Format { get; set; }
    }
}
