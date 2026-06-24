using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace DataSonificationConfiguration
{
    public class Arranger : DataSonificationDBObject
    {
        public string Type
        {
            get { return TableName; }
        }

        private static Dictionary<string, XElement> arrangerTypes = DBObjectTypes.ArrangerTypes;
        public static Arranger FindArranger(string tableName, int id, DataSonificationDB db)
        {
            if (!arrangerTypes.ContainsKey(tableName))
                throw new ArgumentException("Invalid Arranger Type: " + tableName);

            return new Arranger(tableName, id, db);
        }

        public static Arranger CreateArranger(string tableName, int id, int basePitch, DataSonificationDB db)
        {
            if (!arrangerTypes.ContainsKey(tableName))
                throw new ArgumentException("Invalid Arranger Type: " + tableName);

            Dictionary<string, object> defaultValues = new Dictionary<string, object>();
            defaultValues[TickerIdKey] = id;
            defaultValues["base_pitch"] = basePitch;
            AddDefaultAttributes(arrangerTypes[tableName], defaultValues);

            return new Arranger(tableName, defaultValues, db);

        }

        
        private Arranger(string tableName, Dictionary<string, object> defaultValues, DataSonificationDB db)
            : base(tableName, TickerIdKey, db)
        {
            InitializeProperties(defaultValues);
        }

        private Arranger(string tableName, int id, DataSonificationDB db)
            : base(tableName, TickerIdKey, id, db)
        {
            
        }
    }
}
