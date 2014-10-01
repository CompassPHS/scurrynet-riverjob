using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Beds
{
    public abstract class Bed
    {
        public Contexts.RiverContext Context { get; private set; }

        public Bed(Contexts.RiverContext context)
        {
            Context = context;

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
        
        public IObservable<JObject> Convert(object obj)
        {
            return Observable.Return<JObject>(JObject.FromObject(obj));
        }
    }
}
