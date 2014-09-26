using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Beds
{
    public abstract class Bed
    {
        public Contexts.Beds.Bed Context { get; private set; }

        public Bed(Contexts.Beds.Bed bed)
        {
            Context = bed;

            Processor = obj =>
            {
                return Process(obj);
            };

            Converter = obj =>
            {
                return Convert(obj);
            };
        }

        public Func<object, IObservable<object>> Processor { get; private set; }
        public abstract IObservable<object> Process(object obj);
        public Func<object, IObservable<JObject>> Converter { get; private set; }
        public abstract IObservable<JObject> Convert(object obj);
    }
}
