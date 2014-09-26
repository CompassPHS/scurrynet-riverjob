using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;

namespace Scurry.Jobs.RiverJob.Sources
{
    public class FlatFile : Source
    {
        protected Contexts.Sources.FlatFile Context { get; set; }

        public FlatFile(Contexts.Sources.FlatFile context)
            : base()
        {
            Context = context;
        }

        public override IObservable<object> Read()
        {
            switch (Context.Format)
            {
                case "Row":
                    return Observable.Create<List<Dictionary<string, object>>>(o=>
	                {
		                using (var reader = new System.IO.StreamReader(Context.Location))
                        {
                            var cur = new List<Dictionary<string, object>>();
                            string id = null;
                            string line = null;
                            string[] columns = null;

                            while ((line = reader.ReadLine()) != null)
                            {
                                if (columns == null)
                                    columns = ParseLine(line, Context.Delimiters);
                                else
                                {
                                    var row = MakeRowObj(ParseLine(line, Context.Delimiters), columns);

                                    if (row.ContainsKey("_id") && row["_id"].ToString() != id)
                                    {
                                        if (cur.Count > 0) o.OnNext(cur);
                                        cur = new List<Dictionary<string, object>>();
                                        cur.Add(row);
                                        if (row.ContainsKey("_id"))
                                            id = row["_id"].ToString();
                                        else
                                            id = null;
                                    }
                                    else
                                    {
                                        cur.Add(row);
                                    }
                                }
                            }

                            if (cur.Count > 0) o.OnNext(cur); // Last object if exists
                        }
		        
                        o.OnCompleted();
		                return Disposable.Empty;
	                });
                default:
                    throw new ArgumentException(
                        string.Format("Format {0} not supported by FlatFile source", Context.Format));
            }
        }

        private string[] ParseLine(string line, char[] delimiters)
        {
            return line.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
        }

        private Dictionary<string, object> MakeRowObj(string[] values, string[] headers)
        {
            if (headers.Length != values.Length)
                throw new ArgumentOutOfRangeException("values"
                    , string.Format("headers:{0}, values:{1}", headers.Length, values.Length)
                    , "The number of values did not match the number of headers");

            var rowObj = new Dictionary<string, object>();

            for (var i = 0; i < headers.Length; i++ )
                rowObj.Add(headers[i], values[i]);

            return rowObj;
        }
    }
}
