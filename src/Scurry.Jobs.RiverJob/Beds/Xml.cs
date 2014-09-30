using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Scurry.Jobs.RiverJob.Beds
{
    public class Xml : Bed
    {
        public Xml(Contexts.Beds.Bed bed)
            : base(bed)
        {

        }

        public override IObservable<object> Process(object obj)
        {
            return Observable.Return<object>(Processing(obj));
        }

        private object Processing(object obj)
        {
            switch (Context.Format)
            {
                case "Xml":
                    var xml = obj as XPathNavigator;

                    if (xml == null)
                        throw new ArgumentException(
                            "Could not convert input object to XmlDocument");

                    return ProcessXml(xml);
                default:
                    throw new ArgumentException(
                        string.Format("Format {0} not supported by xml bed", Context.Format));
            }
        }

        static Dictionary<string, object> ProcessXml(XPathNavigator xml)
        {
            var cur = new Dictionary<string, object>();

            foreach (XPathNavigator child in xml.SelectChildren(XPathNodeType.All))
            {
                Process(child, cur);
            }

            return cur;
        }

        static void Process(XPathNavigator xml, Dictionary<string, object> parent)
        {
            XPathNodeIterator iter = null;

            // Sub object (name ends with .)
            if (xml.HasChildren
                && xml.SelectChildren(XPathNodeType.Text).Count == 0
                && xml.Name.EndsWith("."))
            {
                var child = new Dictionary<string, object>();

                foreach (XPathNavigator xmlSubChild in xml.SelectChildren(XPathNodeType.All))
                {
                    Process(xmlSubChild, child);
                }

                parent.Add(xml.Name.Replace(".", ""), child);
            }

            // Array of primitives
            else if (xml.HasChildren
                && xml.SelectChildren(XPathNodeType.Element).Count > 0
                && (iter = xml.SelectChildren(XPathNodeType.Element)) != null
                && iter.MoveNext()
                && iter.Current.SelectChildren(XPathNodeType.Text).Count == 1)
            {
                var lst = new List<object>();

                foreach (XPathNavigator child in xml.SelectChildren(XPathNodeType.Element))
                {
                    lst.Add(child.TypedValue);
                }

                parent.Add(xml.Name, lst);
            }

            // Array of objects
            else if (xml.HasChildren
                && xml.SelectChildren(XPathNodeType.Element).Count > 0
                && (iter = xml.SelectChildren(XPathNodeType.Element)) != null
                && iter.MoveNext()
                && iter.Current.Name.EndsWith("."))
            {
                var lst = new List<Dictionary<string, object>>();

                foreach (XPathNavigator xmlChild in xml.SelectChildren(XPathNodeType.Element))
                {
                    var child = new Dictionary<string, object>();

                    foreach (XPathNavigator xmlGrandChild in xmlChild.SelectChildren(XPathNodeType.Element))
                    {
                        Process(xmlGrandChild, child);
                    }

                    lst.Add(child);
                }

                parent.Add(xml.Name.Replace(".", ""), lst);
            }

            // Data
            else if (xml.HasChildren
                && xml.SelectChildren(XPathNodeType.Text).Count == 1)
            {
                parent.Add(xml.Name, xml.TypedValue);
            }
        }
    }
}