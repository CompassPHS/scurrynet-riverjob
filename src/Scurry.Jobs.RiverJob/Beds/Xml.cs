using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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
                    var xml = obj as XmlNode;

                    if (xml == null)
                        throw new ArgumentException(
                            "Could not convert input object to XmlDocument");

                    return ProcessXml(xml);
                default:
                    throw new ArgumentException(
                        string.Format("Format {0} not supported by xml bed", Context.Format));
            }
        }

        static Dictionary<string, object> ProcessXml(XmlNode xml)
        {
            var cur = new Dictionary<string, object>();

            foreach (XmlNode child in xml.ChildNodes)
            {
                ProcessXml(child, cur);
            }

            return cur;
        }

        static void ProcessXml(XmlNode xml, Dictionary<string, object> parent)
        {
            // Sub object (name ends with .)
            if (xml.HasChildNodes
                && xml.FirstChild.NodeType != XmlNodeType.Text
                && xml.Name.EndsWith("."))
            {
                var child = new Dictionary<string, object>();

                foreach (XmlNode xmlSubChild in xml.ChildNodes)
                {
                    ProcessXml(xmlSubChild, child);
                }

                parent.Add(xml.Name.Replace(".", ""), child);
            }

            // Array of primitives
            else if (xml.ChildNodes.Count > 0
                && xml.FirstChild.NodeType != XmlNodeType.Text
                && xml.FirstChild.HasChildNodes
                && xml.FirstChild.FirstChild.NodeType == XmlNodeType.Text)
            {
                var lst = new List<string>();

                foreach (XmlNode child in xml.ChildNodes)
                {
                    lst.Add(child.InnerText);
                }

                parent.Add(xml.Name, lst);
            }

            // Array of objects
            else if (xml.ChildNodes.Count > 0
                && xml.FirstChild.NodeType != XmlNodeType.Text
                && xml.FirstChild.Name.EndsWith("."))
            {
                var lst = new List<Dictionary<string, object>>();

                foreach (XmlNode xmlChild in xml.ChildNodes)
                {
                    var child = new Dictionary<string, object>();

                    foreach (XmlNode xmlGrandChild in xmlChild.ChildNodes)
                    {
                        ProcessXml(xmlGrandChild, child);
                    }

                    lst.Add(child);
                }

                parent.Add(xml.Name.Replace(".", ""), lst);
            }

            // Data
            else if (xml.ChildNodes.Count == 1
                && xml.FirstChild.NodeType == XmlNodeType.Text)
            {
                parent.Add(xml.Name, xml.FirstChild.InnerText);
            }
        }
    }
}