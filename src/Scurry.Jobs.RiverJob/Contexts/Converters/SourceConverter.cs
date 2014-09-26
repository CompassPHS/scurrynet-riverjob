using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Scurry.Jobs.RiverJob.Contexts.Sources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Contexts.Converters
{
    public class SourceConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(Source).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject item = JObject.Load(reader);
        
            switch (item["type"].Value<string>().ToLower())
            {
                case "database":
                    return item.ToObject<Database>();
                case "flatfile":
                    return item.ToObject<FlatFile>();
                default:
                    throw new InvalidCastException();
            }

        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
