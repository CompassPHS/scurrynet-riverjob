using Scurry.Jobs.RiverJob.Beds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Mouths
{
    public static class MouthFactory
    {
        public static Mouth Get(Contexts.Mouths.Mouth context)
        {
            return new Mouth(context);
        }
    }
}
