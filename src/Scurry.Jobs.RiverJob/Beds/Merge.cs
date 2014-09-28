using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Scurry.Jobs.RiverJob.Beds
{
    public class Merge : Bed
    {
        public Merge(Contexts.Beds.Bed bed)
            : base(bed)
        {

        }

        public override IObservable<object> Process(object obj)
        {
            return Observable.Return<object>(Processing(obj));
        }

        private object Processing(object obj)
        {
            switch(Context.Format)
            {
                case "Row":
                    var srcLst = obj as List<Dictionary<string, object>>;

                    if (srcLst == null)
                        throw new ArgumentException(
                            "Could not convert input object to List<Dictionary<string, object>>");

                    var dest = new Dictionary<string, object>();

                    foreach (var src in srcLst)
                    {
                        ProcessMerge(src, dest);
                    }

                    return dest;
                default:
                    throw new ArgumentException(
                        string.Format("Format {0} not supported by merge bed", Context.Format));
            }
        }

        private void ProcessMerge(Dictionary<string, object> src, Dictionary<string, object> dest)
        {
            foreach (var skvp in src)
            {
                if (!dest.ContainsKey(skvp.Key))
                {
                    dest.Add(skvp.Key, skvp.Value);
                }
                else if (skvp.Value.GetType() == typeof(Dictionary<string, object>))
                {
                    ProcessMerge(skvp.Value as Dictionary<string, object>, dest[skvp.Key] as Dictionary<string, object>);
                }
                else if (skvp.Value.GetType() == typeof(List<Dictionary<string, object>>))
                {
                    var srcList = skvp.Value as List<Dictionary<string, object>>;
                    var destList = dest[skvp.Key] as List<Dictionary<string, object>>;

                    foreach (var srcChild in srcList)
                    {
                        Dictionary<string, object> destMatch = null;
                        foreach (var destChild in destList)
                        {
                            if (destChild.ContainsKey("_id") && srcChild.ContainsKey("_id")
                                && destChild["_id"].ToString() == srcChild["_id"].ToString())
                            {
                                destMatch = destChild;
                                break;
                            }
                        }

                        if (destMatch != null) ProcessMerge(srcChild, destMatch);
                        else destList.Add(srcChild);
                    }
                }
                else if (skvp.Value.GetType() == typeof(List<object>))
                {
                    var srcList = skvp.Value as List<object>;
                    var destList = dest[skvp.Key] as List<object>;

                    foreach (var srcChild in srcList)
                    {
                        if (!destList.Contains(srcChild))
                            destList.Add(srcChild);
                    }
                }
                else
                {
                    dest[skvp.Key] = skvp.Value;
                }
            }
        }
    }
}
