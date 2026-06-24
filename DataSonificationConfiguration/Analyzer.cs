using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataSonificationConfiguration
{
    public class Analyzer : DataSonificationDBObject
    {
        public string Type
        {
            get { return TableName; }
        }

        private static Dictionary<string, XElement> analyzerTypes = DBObjectTypes.AnalyzerTypes;
        public static Analyzer FindAnalyzer(string tableName, int id, DataSonificationDB db)
        {

            return new Analyzer(tableName, id, db);
        }

        public static Analyzer CreateAnalyzer(string tableName, int id, DataSonificationDB db)
        {
            if (!analyzerTypes.ContainsKey(tableName))
                throw new ArgumentException("Invalid Analyzer Type: " + tableName);

            Dictionary<string, object> defaultValues = new Dictionary<string, object>();
            defaultValues[TickerIdKey] = id;
            AddDefaultAttributes(analyzerTypes[tableName], defaultValues);

            return new Analyzer(tableName, defaultValues, db);

        }

        
        private Analyzer(string tableName, Dictionary<string, object> defaultValues, DataSonificationDB db)
            : base(tableName, TickerIdKey, db)
        {
            InitializeProperties(defaultValues);
        }

        private Analyzer(string tableName, int id, DataSonificationDB db)
            : base(tableName, TickerIdKey, id, db)
        {
            
        }
    }
}
