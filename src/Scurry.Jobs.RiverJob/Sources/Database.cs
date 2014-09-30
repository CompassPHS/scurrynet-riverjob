using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Scurry.Jobs.RiverJob.Sources
{
    public class Database : Source
    {
        protected Contexts.Sources.Database Context { get; set; }

        public Database(Contexts.Sources.Database context)
            : base()
        {
            Context = context;
        }

        public override IObservable<object> Read()
        {
            switch (Context.Format)
            {
                case "Row":
                    return Observable.Create<List<Dictionary<string, object>>>(o =>
                    {
                        using (var connection = new SqlConnection(Context.ConnectionString))
                        {
                            var cur = new List<Dictionary<string, object>>();
                            string id = null;
                            connection.Open();

                            using (var cmd = new SqlCommand(Context.Command, connection))
                            {
                                cmd.CommandTimeout = Context.CommandTimeout;

                                using (var reader = cmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        var row = new Dictionary<string, object>();

                                        for (int i = 0; i < reader.FieldCount; i++)
                                        {
                                            var data = reader[i];

                                            if (Context.SuppressNulls && data == DBNull.Value) continue;

                                            ParseColumn(reader.GetName(i), data, row);
                                        }

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

                                    if (cur.Count > 0) o.OnNext(cur); // Last object if exists
                                }
                            }
                        }

                        o.OnCompleted();
                        return Disposable.Empty;
                    });
                case "Xml":
                    return Observable.Create<XPathNavigator>(o =>
                    {
                        using (var connection = new SqlConnection(Context.ConnectionString))
                        {
                            connection.Open();
                            var more = true;

                            while (more)
                            {
                                using (var cmd = new SqlCommand(Context.Command, connection))
                                {
                                    cmd.CommandTimeout = Context.CommandTimeout;

                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        StringBuilder xmlschema = new StringBuilder(string.Empty);

                                        while (reader.Read())
                                        {
                                            xmlschema.Append(reader.GetString(0));
                                        }

                                        XmlReader schemaReader = XmlReader.Create(new StringReader(xmlschema.ToString()));
                                        var settings = new XmlReaderSettings();
                                        settings.Schemas.Add(null, schemaReader);
                                        settings.ValidationType = ValidationType.Schema;

                                        if (reader.NextResult())
                                        {
                                            StringBuilder xmlsb = new StringBuilder(string.Empty);

                                            while (reader.Read())
                                            {
                                                xmlsb.Append(reader.GetString(0));
                                            }

                                            var doc = new XmlDocument();
                                            XmlReader xmlReader = XmlReader.Create(new StringReader(xmlsb.ToString()), settings);
                                            doc.Load(xmlReader);
                                            var nav = doc.CreateNavigator();
                                            var objs = nav.Select("/index/type");

                                            foreach (XPathNavigator obj in objs)
                                            {
                                                o.OnNext(obj);
                                            }

                                            // More results?
                                            more = false;

                                            if (reader.NextResult())
                                            {
                                                if (reader.Read())
                                                {
                                                    more = reader.GetBoolean(0);
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            o.OnCompleted();
                            return Disposable.Empty;
                        }
                    });
                default:
                    throw new ArgumentException(
                        string.Format("Format {0} not supported by Database source", Context.Format));
            }
        }

        private void ParseColumn(string column, object data, Dictionary<string, object> parentObj)
        {
            // First child is property
            if ((column.IndexOf('.') > -1 && column.IndexOf('[') > -1 && column.IndexOf('.') < column.IndexOf('['))
                || (column.IndexOf('.') > -1 && column.IndexOf(']') == -1))
            {
                var idx = column.IndexOf('.');
                var name = column.Substring(0, idx);

                if (!parentObj.ContainsKey(name))
                    parentObj[name] = new Dictionary<string, object>();

                ParseColumn(column.Substring(idx + 1).Trim(), data, (parentObj[name] as Dictionary<string, object>));
            }
            // First child is array of primitives
            else if (column.IndexOf('[') > -1 && column.IndexOf(']') == column.IndexOf('[') + 1)
            {
                var idx = column.IndexOf('[');
                var name = column.Substring(0, idx);

                if (!parentObj.ContainsKey(name))
                    parentObj[name] = new List<object>() { data };
                else
                {
                    var list = parentObj[name] as List<object>;
                    if (!list.Contains(data)) list.Add(data);
                }
            }
            // First child is array of objects
            else if ((column.IndexOf('[') > -1 && column.IndexOf('.') > -1 && column.IndexOf('[') < column.IndexOf('.'))
                || (column.IndexOf('[') > -1 && column.IndexOf('.') == -1))
            {
                var idx = column.IndexOf('[');
                var name = column.Substring(0, idx);

                var childName = column.Substring(idx + 1);

                if ((childName.IndexOf(']') > -1 && childName.IndexOf('[') > -1 && childName.IndexOf(']') < childName.IndexOf('['))
                    || (childName.IndexOf(']') > -1 && childName.IndexOf('[') == -1))
                {
                    var remove = childName.IndexOf(']');
                    childName = childName.Substring(0, remove) + childName.Substring(remove + 1, childName.Length - remove - 1);
                }

                if (!parentObj.ContainsKey(name))
                    parentObj[name] = new List<Dictionary<string, object>>() { new Dictionary<string, object>() };

                ParseColumn(childName, data, (parentObj[name] as List<Dictionary<string, object>>)[0] as Dictionary<string, object>);

            }
            // No children
            else
            {
                parentObj[column] = data;
            }
        }
    }
}
