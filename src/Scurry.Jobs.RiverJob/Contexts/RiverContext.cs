using Newtonsoft.Json;
using Scurry.Jobs.RiverJob.Contexts.Sources;
using Scurry.Jobs.RiverJob.Contexts.Mouths;
using Scurry.Jobs.RiverJob.Contexts.Beds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Contexts
{
    [Serializable]
    [JsonObject]
    public class RiverContext
    {
        public string Name { get; set; }

        public Source Source { get; set; }

        public Bed Bed { get; set; }

        public Mouth Mouth { get; set; }

        public string Cron { get; set; }
        
        public RiverContext()
        {
            
        }
    }    
}
