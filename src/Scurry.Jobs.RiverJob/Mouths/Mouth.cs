using Common.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scurry.Jobs.RiverJob.Contexts;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Mouths
{
    public class Mouth
    {
        ILog log = Common.Logging.LogManager.GetCurrentClassLogger();

        Contexts.Mouths.Mouth _destination;
        Nest.ElasticClient _client;
        bool _indexCreated = false;
        StringBuilder sb = new StringBuilder();
        int count = 0;
        int start = System.Environment.TickCount;
        
        public Mouth(Contexts.Mouths.Mouth destination)
        {
            _destination = destination;
            _client = new Nest.ElasticClient(new Nest.ConnectionSettings(new Uri(destination.Url)));
        }

        public void Collect(JObject curObj)
        {
            Collect(curObj, false);
        }

        public void Collect(JObject curObj, bool expelNow)
        {
            if (!_indexCreated) EagerCreateIndex(_destination);

            var settings = new Newtonsoft.Json.JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            var index = _destination.Index;
            var type = _destination.Type;
            sb.Append("{ \"index\" : { \"_index\" : \"" + index + "\", \"_type\" : \"" + type + "\"");
            if (curObj["_id"] != null) sb.Append(", \"_id\" : \"" + curObj["_id"] + "\"");
            if (curObj["_parent"] != null) sb.Append(", \"_parent\" : \"" + curObj["_parent"] + "\"");
            sb.Append(" } }");
            sb.Append("\n");
            sb.Append(JsonConvert.SerializeObject(curObj, settings));
            sb.Append("\n");

            count++;

            if (expelNow || count % _destination.MaxBulkSize == 0)
            {
                Expel();
            }
        }

        public void Expel()
        {
            // Send the current state on regardless
            BulkPushToElasticsearch(sb.ToString());
            sb.Clear();
            var end = System.Environment.TickCount;
            log.Info(string.Format("{0} has taken {1}s", count, (end - start) / 1000));
        }

        private void EagerCreateIndex(Contexts.Mouths.Mouth destination)
        {
            try
            {
                var index = _client.CreateIndex(c => c
                    .Index(destination.Index)
                    .InitializeUsing(new Nest.IndexSettings())
                    );

                _indexCreated = true;

                if (destination.Mapping != null)
                {
                    _client.Map<object>(m =>
                    {
                        m.Index(destination.Index);
                        m.Type(destination.Type);

                        if (destination.Mapping.Parent != null)
                            m.SetParent(destination.Mapping.Parent.Type);

                        return m;
                    });
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                throw;
            }
        }

        private void BulkPushToElasticsearch(string body)
        {
            var connectionStatus = _client.Raw.BulkPut(_destination.Index
                , _destination.Type
                , body
                , null);

            if (connectionStatus.Success)
            {
                log.Debug(connectionStatus);
                log.Info(string.Format("Result:{0}", connectionStatus.Success));
            }
            else
            {
                log.Error(connectionStatus.ServerError.Error);
            }
        }
    }
}