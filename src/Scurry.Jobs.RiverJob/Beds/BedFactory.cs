using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Beds
{
    public static class BedFactory
    {
        public static Bed Get(Contexts.RiverContext context)
        {
            switch (context.Bed.Strategy)
            {
                case "Merge":
                    return new Merge(context);
                case "Xml":
                    return new Xml(context);
                default:
                    throw new ArgumentException();
            }
        }
    }
}
