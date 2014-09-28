using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Beds
{
    public static class BedFactory
    {
        public static Bed Get(Contexts.Beds.Bed bed)
        {
            switch (bed.Strategy)
            {
                case "Merge":
                    return new Merge(bed);
                case "Xml":
                    return new Xml(bed);
                default:
                    throw new ArgumentException();
            }
        }
    }
}
