using Common.Logging;
using Newtonsoft.Json;
using Scurry.Jobs.RiverJob.Contexts;
using Scurry.Jobs.RiverJob.Contexts.Converters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob
{
    public class River : Scurry.Executor.Job.Base.Job
    {
        ILog log = Common.Logging.LogManager.GetCurrentClassLogger();

        public River()
            : base("river")
        {
            
        }

        public override void Execute(dynamic context)
        {
            var riverContext =
                JsonConvert.DeserializeObject<RiverContext>(context.ToString(), new SourceConverter());
            Flow(riverContext);
        }

        public void Flow(RiverContext riverContext)
        {
            try
            {
                log.Info(string.Format("Configuring river {0}", riverContext.Name));

                var reset = new AutoResetEvent(false);
                var source = Sources.SourceFactory.Get(riverContext.Source);
                var bed = Beds.BedFactory.Get(riverContext);
                var mouth = Mouths.MouthFactory.Get(riverContext.Mouth);

                var flow = from iObj in source.Read()
                           from cObj in bed.Processor(iObj)
                           from jObj in bed.Converter(cObj)
                           select jObj;

                log.Info(string.Format("Starting river {0}", riverContext.Name));

                using (flow.SubscribeOn(Scheduler.Default).Subscribe(
                    drop =>
                    {
                        mouth.Collect(drop);
                    },
                    ex =>
                    {
                        log.ErrorFormat("Error while processing river: {0}", ex);
                        throw new ApplicationException("Error while processing river", ex);
                    },
                    () =>
                    {
                        mouth.Expel();
                        reset.Set();
                    }
                ))

                reset.WaitOne();

                log.Info(string.Format("Completed river {0}", riverContext.Name));
            }
            catch (Exception e)
            {
                log.Error(string.Format("Error river {0}", riverContext.Name), e);
            }
        }
    }
}
