using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace DataSonificationConfiguration
{
    public static class DBObjectTypes
    {
        private static XElement typesXml;

        static DBObjectTypes()
        {
            typesXml = (XElement)XElement.Parse(Resources.Types);
            
        }

        public static Dictionary<string, XElement> AnalyzerTypes
        {
            get
            {
                Dictionary<string, XElement> dict = new Dictionary<string, XElement>();
                var types = from analyzer in typesXml.Descendants("analyzer") select analyzer;
                foreach (var type in types)
                {
                    dict[type.Attribute("tableName").Value] = type;
                }
                return dict;
            }
        }


        public static Dictionary<string, XElement> ArrangerTypes
        {
            get
            {
                Dictionary<string, XElement> dict = new Dictionary<string, XElement>();
                var types = from analyzer in typesXml.Descendants("arranger") select analyzer;
                foreach (var type in types)
                {
                    dict[type.Attribute("tableName").Value] = type;
                }
                return dict;
            }
        }

        public static Dictionary<string, XElement> InstrumentTypes
        {
            get
            {
                Dictionary<string, XElement> dict = new Dictionary<string,XElement>();
                var types = from analyzer in typesXml.Descendants("instrument") select analyzer;
                foreach(var type in types)
                {
                    dict[type.Attribute("tableName").Value] = type;
                }
                return dict;
            }
        }

        public static Dictionary<string, XElement> TrainerTypes
        {
            get
            {
                Dictionary<string, XElement> dict = new Dictionary<string, XElement>();
                var types = from analyzer in typesXml.Descendants("trainer") select analyzer;
                foreach (var type in types)
                {
                    dict[type.Attribute("tableName").Value] = type;
                }
                return dict;
            }
        }
    }
}
