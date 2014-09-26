using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Sources
{
    public static class SourceFactory
    {
        public static Source Get(Contexts.Sources.Source source)
        {
            var contextType = source.GetType();
            if (contextType == typeof(Contexts.Sources.Database))
                return new Database(source as Contexts.Sources.Database);
            else if (contextType == typeof(Contexts.Sources.FlatFile))
                return new FlatFile(source as Contexts.Sources.FlatFile);
            else
                throw new ArgumentException();
        }
    }
}
